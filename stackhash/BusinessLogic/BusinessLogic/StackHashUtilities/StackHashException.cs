//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace StackHashUtilities
//{
//    public enum StackHashExceptionSource
//    {
//        Unknown = 0,
//        BusinessLogic,
//        WindowsService,
//        WcfServices,
//        Client
//    }

//    [Serializable]
//    public class StackHashException : Exception
//    {
//        private StackHashExceptionSource m_Source;

//        public StackHashExceptionSource SourceModule
//        {
//            get { return m_Source; }
//        }

//        /// <summary>
//        /// Default constructor.
//        /// </summary>

//        public StackHashException()
//        {
//        }


//        /// <summary>
//        /// Indicates an error specific to the StackHash application.
//        /// </summary>
//        /// <param name="message">Message to be included in the exception.</param>

//        public StackHashException(string message): base(message)
//        {
//        }


//        /// <summary>
//        /// Indicates an error specific to the StackHash application.
//        /// </summary>
//        /// <param name="message">Message to be included in the exception.</param>
//        /// <param name="ex">Inner exception.</param>


//        public StackHashException(string message, System.Exception ex)
//            : base(message, ex)
//        {
//        }


//        /// <summary>
//        /// Indicates an error specific to the StackHash application.
//        /// </summary>
//        /// <param name="message">Message to be included in the exception.</param>
//        /// <param name="source">Component that generated the exception.</param>
//        /// <param name="ex">Inner exception.</param>

//        public StackHashException(string message, StackHashExceptionSource source, System.Exception ex) :
//            base(message, ex)
//        {
//            m_Source = source;
//        }


//        /// <summary>
//        /// Check if the specified exception contains a particular source. 
//        /// Searches all nested exceptions.
//        /// </summary>
//        /// <param name="ex">The exception to check.</param>
//        /// <param name="source">The source to look for.</param>
//        /// <returns>The first exception of the source specified or null</returns>

//        public static System.Exception FindInnerException(System.Exception ex, StackHashExceptionSource source)
//        {
//            while (ex != null)
//            {
//                if (ex.GetType() == typeof(StackHashException))
//                {
//                    StackHashException stackHashException = ex as StackHashException;

//                    if (stackHashException.SourceModule == source)
//                        return stackHashException;
//                }
//                ex = ex.InnerException;
//            }
//            return null;
//        }
//    }
//}
