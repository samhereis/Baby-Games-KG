using System;

namespace DataClasses
{
    [Serializable]
    public class KeyedObject<T_Key, T_Object>
    {
        public T_Key key;
        public T_Object value;

        public KeyedObject(T_Key t_Key, T_Object t_Object)
        {
            key = t_Key;
            value = t_Object;
        }
    }
}
