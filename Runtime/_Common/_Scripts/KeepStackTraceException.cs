using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    public class KeepStackTraceException : Exception
    {
        public KeepStackTraceException(Exception e) : base(e.Message, e)
        {

        }
        public override string StackTrace
        {
            get
            {
                if (InnerException != null)
                {
                    return InnerException.StackTrace;
                }
                return base.StackTrace;
            }
        }
    }
}
