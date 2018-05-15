using System;

namespace LecturerTrainer.Model
{
    /// <summary>
    /// Class that allows to modelize a KeyValuePair
    /// </summary>
    [Serializable()]
    public class KeyValuePairPerso<K, V>
    {
        public K Key { get; set; }
        public V Value { get; set; }

        public KeyValuePairPerso(K tKey, V tValue)
        {
            Key = tKey;
            Value = tValue;
        }

        public KeyValuePairPerso()
        {
        }
    }
}
