using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Nextension.TextureLoader
{
    public class Texture2DLoaderOperation : NOperation
    {
        public int OriginWidth { get; private set; }
        public int OriginHeight { get; private set; }
        public Texture2D Texture { get; private set; }
        public ImageExtension ImageExtension { get; internal set; }

        internal void setResult(Texture2D texture, int originWidth, int originHeight)
        {
            Texture = texture;
            OriginWidth = originWidth;
            OriginHeight = originHeight;
            innerFinalize();
        }
        internal void setError(Exception e)
        {
            innerFinalize(e);
        }
    }

    internal abstract class AbsProcessTask : NOperation, ISchedulable
    {
        protected byte[] _imageData;
        protected Uri _uri;
        protected string _url;

        internal int priority;

        int ISchedulable.Priority => priority;
        void ISchedulable.onCanceled()
        {
            if (Status != RunState.None)
            {
                Debug.LogWarning("Can't cancel started task!");
                return;
            }
            Status = RunState.Canceled;
            var e = new OperationCanceledException();
            _processOperation.setError(e);
            innerFinalize(e);
        }
        void ISchedulable.onStartExecute()
        {
            startProcess();
        }

        public void initialize(byte[] imageData, TextureSetting setting)
        {
            _setting = setting;
            _imageData = imageData;
            _uri = null;
            _url = null;
            if (_setting == null)
            {
                _setting = new TextureSetting();
            }
            _processOperation = new Texture2DLoaderOperation();
            Status = RunState.None;
        }
        public void initialize(Uri uri, TextureSetting setting)
        {
            _setting = setting;
            _imageData = null;
            _uri = uri;
            _url = null;
            if (_setting == null)
            {
                _setting = new TextureSetting();
            }
            _processOperation = new Texture2DLoaderOperation();
            Status = RunState.None;
        }
        public void initialize(string url, TextureSetting setting)
        {
            _setting = setting;
            _imageData = null;
            _url = null;
            _url = url;
            if (_setting == null)
            {
                _setting = new TextureSetting();
            }
            _processOperation = new Texture2DLoaderOperation();
            Status = RunState.None;
        }

        private Texture2DLoaderOperation _processOperation;
        private RunState Status { get; set; }
        protected TextureSetting _setting;

        public void startProcess()
        {
            if (Status == RunState.None)
            {
                Status = RunState.Running;
                try
                {
                    if (_imageData != null)
                    {
                        exeProcess(_imageData);
                    }
                    else if (_uri != null)
                    {
                        exeProcess(_uri);
                    }
                    else
                    {
                        exeProcess(new Uri(_url));
                    }
                }
                catch (Exception ex)
                {
                    setError(ex);
                }
            }
        }

        public Texture2DLoaderOperation getOperation() => _processOperation;

        protected abstract NTask exeProcess(byte[] inData);
        protected abstract NTask exeProcess(Uri uri);

        protected void setImageExtension(ImageExtension extension)
        {
            _processOperation.ImageExtension = extension;
        }
        public void setResult(Texture2D texture2D, int width, int height)
        {
            if (IsFinalized)
            {
                return;
            }
            Status = RunState.Completed;
            _processOperation.setResult(texture2D, width, height);
            innerFinalize();
        }
        public void setError(Exception e)
        {
            if (IsFinalized)
            {
                return;
            }
            Status = RunState.Exception;
            _processOperation.setError(e);
            innerFinalize(e);
        }
    }
}
