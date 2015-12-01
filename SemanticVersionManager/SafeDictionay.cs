namespace SemanticVersionManager
{
    using System;
    using System.Collections.Generic;

    public class SafeDictionay<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public SafeDictionay()
        {
        }

        public SafeDictionay(Dictionary<TKey, TValue> values) : base(values)
        {
        }

        public new TValue this[TKey key]
        {
            get
            {
                if (this.ContainsKey(key))
                {
                    return base[key];
                }

                return default(TValue);
            }
            set
            {
                base[key] = value;
            }
        }
    }
}