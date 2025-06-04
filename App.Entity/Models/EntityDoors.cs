using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Entity.Models
{
    public class EntityDoors
    {
    }
    public class AccessRuleRequest
    {
        /// <summary>
        /// The unique identifier for the door (GUID or logical ID).
        /// </summary>
        public string Door { get; set; }

        /// <summary>
        /// The unique identifier for the access rule (GUID or logical ID).
        /// </summary>
        public string AccessRule { get; set; }

        /// <summary>
        /// The side for the access rule. Accepts "In", "Out", or "Both".
        /// </summary>
        public string Side { get; set; }
    }
    public class DoorAccessRulesRequest
    {
        /// <summary>
        /// The unique identifier for the door (GUID or logical ID).
        /// </summary>
        public string Door { get; set; }
    }
    public class DoorAccessRulesSideRequest
    {
        /// <summary>
        /// The unique identifier for the door (GUID or logical ID).
        /// </summary>
        public string Door { get; set; }

        /// <summary>
        /// The side of access rule, e.g., In, Out, or Both.
        /// </summary>
        public string AccessRuleSide { get; set; }
    }
    public class DoorAccessRuleRemovalRequest
    {
        /// <summary>
        /// The unique identifier for the door (GUID or logical ID).
        /// </summary>
        public string Door { get; set; }

        /// <summary>
        /// The unique identifier for the access rule to remove (GUID or logical ID).
        /// </summary>
        public string AccessRule { get; set; }
    }
    public class DoorConnectionRequest
    {
        /// <summary>
        /// The unique identifier for the door (GUID or logical ID).
        /// </summary>
        public string Door { get; set; }

        /// <summary>
        /// The unique identifier for the device (GUID or logical ID).
        /// </summary>
        public string Device { get; set; }

        /// <summary>
        /// The type of access point (e.g., CardReader, DoorSensor, Buzzer).
        /// </summary>
        public string AccessPointType { get; set; }
    }
    public class UpdateConnectionRequest
    {
        /// <summary>
        /// The unique identifier for the door (GUID or logical ID).
        /// </summary>
        public string Door { get; set; }

        /// <summary>
        /// The unique identifier for the access point (GUID or logical ID).
        /// </summary>
        public string AccessPoint { get; set; }

        /// <summary>
        /// The unique identifier for the new device (GUID or logical ID).
        /// </summary>
        public string Device { get; set; }
    }
    public class RemoveConnectionRequest
    {
        /// <summary>
        /// The unique identifier for the door (GUID or logical ID).
        /// </summary>
        public string Door { get; set; }

        /// <summary>
        /// The unique identifier for the access point (GUID or logical ID).
        /// </summary>
        public string AccessPoint { get; set; }
    }
    public class RemoveConnectionByDeviceRequest
    {
        /// <summary>
        /// The unique identifier for the door (GUID or logical ID).
        /// </summary>
        public string Door { get; set; }

        /// <summary>
        /// The unique identifier for the device (GUID or logical ID).
        /// </summary>
        public string Device { get; set; }
    }



}
