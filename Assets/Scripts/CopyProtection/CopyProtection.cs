using UnityEngine;
using System;

public class CopyProtection : MonoBehaviour {
    [Header("Protection Parameters")]
    public Device[] authorizedDevices;
    public Date expirationDate;
    
    [Header("What happens on violation")]
    public CopyProtectionUI violationUI;
    public GameObject[] disableGameobjects;

    void Start() {
        var expirationDateTime = expirationDate.dateTime;
        var now = DateTime.Now;
        bool isExpired = now >= expirationDateTime;

        // A very weird bug happens on build when accessing deviceUniqueIdentifier:
        // The game continues working (including the detector, etc...), but most gameObjects
        // get disabled.
        // TODO: find a fix for this
        // string test_deviceID = SystemInfo.deviceUniqueIdentifier;
        // bool isDeviceAuthorized = isAuthorizedDevice(test_deviceID);
        bool isDeviceAuthorized = true;

        Violation violation = null;
        if (isExpired) {
            violation = new Violation() {
                type = ViolationType.EXPIRED,
                message = $"The license for {Application.productName} has expired",
            };
        }
        else if (!isDeviceAuthorized) {
            violation = new Violation() {
                type = ViolationType.EXPIRED,
                message = $"Your device was not registered to run {Application.productName}",
            };
        }

        if (violation != null) {
            foreach (var item in disableGameobjects) {
                item.SetActive(false);
            }
            if (violationUI != null) violationUI.showViolation(violation);

            Debug.LogError(
                $"{violation.message}. Please contact {Application.companyName} to resolve the issue\n"+
                $"  device ID: {SystemInfo.deviceUniqueIdentifier}\n"+
                $"  device name: {SystemInfo.deviceName}\n");
        }
    }

    void Reset() {
        // Default expiration date is today
        expirationDate = Date.fromDateTime(DateTime.Now);
        expirationDate.day += 1;

        // Add current device to the authorized list
        authorizedDevices = new Device[] { new Device() { name=SystemInfo.deviceName, deviceID=SystemInfo.deviceUniqueIdentifier } };
    }

    bool isAuthorizedDevice(string deviceID) {
        for (int i = 0; i < authorizedDevices.Length; i++)
        {
            if (authorizedDevices[i].deviceID == deviceID) return true;
        }
        return false;
    }

    public enum ViolationType {
        EXPIRED, UNREGISTERED_DEVICE,
    }

    public class Violation {
        public string message;
        public ViolationType type;
    }

    [Serializable]
    public class Device {
        public string name;
        public string deviceID;
    }

    [Serializable]
    public class Date {
        public int year = 2021;
        public int month = 1;
        public int day = 1;
        public DateTime dateTime {
            get { return new DateTime(year, month, day); }
        }
        public static Date fromDateTime (DateTime datetime) {
            return new Date() { year=datetime.Year, month=datetime.Month, day=datetime.Day };
        }
    }
}