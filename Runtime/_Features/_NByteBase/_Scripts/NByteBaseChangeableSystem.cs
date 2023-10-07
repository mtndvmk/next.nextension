using System;
using System.Collections.Generic;
using System.Linq;

namespace Nextension.NByteBase
{
    public abstract class NByteBaseChangeable
    {
        protected event Action onDataChangedEvent;
        protected void invokeDataChangedEvent()
        {
            onDataChangedEvent?.Invoke();
        }
        public NByteBaseChangeable addChangedListener(Action callback)
        {
            onDataChangedEvent += callback;
            return this;
        }
        public NByteBaseChangeable removeListener(Action callback)
        {
            onDataChangedEvent -= callback;
            return this;
        }
        public void removeAllListener()
        {
            onDataChangedEvent = null;
        }
        public abstract void setValue(object value);
        public abstract object getValue();
        public abstract Type getValueType();
        public abstract AbsNByteBase createByteBase();
    }

    public class NByteBaseChangeable<T> : NByteBaseChangeable
    {
        internal NByteBaseChangeable()
        {

        }
        public T OldValue { get; protected set; }
        public T CurrentValue { get; protected set; }

        public void setValue(T value)
        {
            if (CurrentValue == null || !CurrentValue.equals(value))
            {
                OldValue = CurrentValue;
                CurrentValue = value;
                invokeDataChangedEvent();
            }
        }
        public override void setValue(object value)
        {
            setValue((T)value);
        }
        public override object getValue()
        {
            return CurrentValue;
        }
        public override Type getValueType()
        {
            return typeof(T);
        }
        public override AbsNByteBase createByteBase()
        {
            var nByteBase = NByteBaseCreator.createNByteBase<T>();
            nByteBase.setValue(CurrentValue);
            return nByteBase;
        }
    }

    public class NByteBaseChangeableSystem
    {
        private Dictionary<short, NByteBaseChangeable> _dataTable = new Dictionary<short, NByteBaseChangeable>();
        private Dictionary<short, NByteBaseChangeable> _changedTable = new Dictionary<short, NByteBaseChangeable>();
        public NByteBaseChangeable<T> register<T>(short id)
        {
            var valueType = typeof(T);
            if (!NByteBaseUtils.isSupportByteBaseType(valueType))
            {
                throw new Exception($"NByteBase isn't support type [{valueType}]");
            }
            if (_dataTable.ContainsKey(id))
            {
                throw new Exception($"Existed id [{id}]");
            }
            var byteBaseChangeable = new NByteBaseChangeable<T>();
            addToDataTable(id, byteBaseChangeable);
            return byteBaseChangeable;
        }
        private void addToDataTable(short id, NByteBaseChangeable byteBaseChangeable)
        {
            byteBaseChangeable.addChangedListener(() => addToChangedTable(id, byteBaseChangeable));
            _dataTable.Add(id, byteBaseChangeable);
        }
        private void addToChangedTable(short id, NByteBaseChangeable byteBaseChangeable)
        {
            if (!_changedTable.ContainsKey(id))
            {
                _changedTable.Add(id, byteBaseChangeable);
            }
        }
        public NByteBaseChangeable[] getChangedTable(bool isClearChangedTable)
        {
            var arr = _changedTable.Values.ToArray();
            if (isClearChangedTable)
            {
                clearChangedTable();
            }
            return arr;
        }
        public NList getChangedNList(bool isClearChangedTable)
        {
            NList nList = new NList();
            var changedDatas = getChangedTable(isClearChangedTable);
            foreach (var dataChanged in changedDatas)
            {
                var nByteBase = dataChanged.createByteBase();
                nList.Value.Add(nByteBase);
            }
            return nList;
        }
        public byte[] getChangedBytes(bool isClearChangedTable)
        {
            var nList = getChangedNList(isClearChangedTable);
            return nList.getBytes();
        }
        public void clearChangedTable()
        {
            _changedTable.Clear();
        }
    }
}
