namespace Utils
{
    public class Singleton<T> where T : new()
    {
        private static object guard = new object();
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (guard)
                    {
                        if (instance == null)
                            instance = new T();
                    }
                }
                return instance;
            }
        }
    }
}