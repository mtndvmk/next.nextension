using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    public interface IWaitable
    {
        internal bool IsWaitable { get; }
        internal Func<CompleteState> buildCompleteFunc();
    }
    public interface IWaitableFromCancellable
    {
        internal bool IsWaitable { get; }
        internal (Func<CompleteState>, ICancellable) buildCompleteFunc();
    }
    public interface IWaitable_Editor : IWaitable
    {

    }
}
