﻿using System;

namespace Treefrog.Framework
{
    public interface IResource
    {
        Guid Uid { get; }
        bool IsModified { get; }

        event EventHandler Modified;

        void ResetModified ();
    }

    public class ResourceEventArgs : EventArgs, IResourceEventArgs
    {
        public static new ResourceEventArgs Empty = new ResourceEventArgs(Guid.Empty);

        public Guid Uid { get; private set; }

        public ResourceEventArgs (Guid uid)
        {
            Uid = uid;
        }
    }

    public interface IResourceEventArgs
    {
        Guid Uid { get; }
    }

    public interface IResourceEventArgs<out T> : IResourceEventArgs
        where T : IResource
    {
        T Resource { get; }
    }

    public class ResourceEventArgs<T> : ResourceEventArgs, IResourceEventArgs<T>
        where T : IResource
    {
        public T Resource { get; private set; }

        public ResourceEventArgs (T resource)
            : base(resource.Uid)
        {
            Resource = resource;
        }
    }
}
