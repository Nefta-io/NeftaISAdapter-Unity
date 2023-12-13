using System.Collections.Generic;

namespace Nefta.Core.Events
{
    public enum SessionCategory
    {
        AccountConnected,
        AccountUpgraded
    }

    public class SessionEvent : GameEvent
    {
        private static readonly Dictionary<SessionCategory, string> CategoryToString = new Dictionary<SessionCategory, string>()
        {
            { SessionCategory.AccountConnected, "account_connected" },
            { SessionCategory.AccountUpgraded, "account_upgraded" }
        };
        
        public SessionCategory _category;
        public string _name;
        public int _value;
        public string _customString;
        
        public override RecordedEvent GetRecordedEvent()
        {
            return new RecordedEvent()
            {
                _type = "session",
                _category = CategoryToString[_category],
                _itemName = _name,
                _value = _value,
                _customPayload = _customString
            };
        }
    }
}