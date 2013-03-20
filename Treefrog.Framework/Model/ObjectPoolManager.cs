﻿using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using Treefrog.Framework.Model.Proxy;

namespace Treefrog.Framework.Model
{
    public interface IObjectPoolManager : IPoolManager<ObjectPool, Guid>
    {
        TexturePool TexturePool { get; }
    }

    public class ObjectPoolManager : PoolManager<ObjectPool, Guid>, IPoolManager<ObjectPool, Guid>
    {
        private TexturePool _texPool;

        public ObjectPoolManager (TexturePool texPool)
            : base()
        {
            _texPool = texPool;
        }

        public TexturePool TexturePool
        {
            get { return _texPool; }
        }

        protected override ObjectPool CreatePoolCore (string name)
        {
            return new ObjectPool(name, this);
        }

        internal override Guid TakeKey ()
        {
            return Guid.NewGuid();
        }

        public static LibraryX.ObjectGroupX ToXmlProxyX (ObjectPoolManager manager)
        {
            if (manager == null)
                return null;

            List<LibraryX.ObjectPoolX> pools = new List<LibraryX.ObjectPoolX>();
            foreach (ObjectPool pool in manager.Pools)
                pools.Add(ObjectPool.ToXmlProxyX(pool));

            return new LibraryX.ObjectGroupX() {
                ObjectPools = pools,
            };
        }

        public static ObjectPoolManager FromXmlProxy (LibraryX.ObjectGroupX proxy, TexturePool texturePool)
        {
            if (proxy == null)
                return null;

            ObjectPoolManager manager = new ObjectPoolManager(texturePool);
            if (proxy.ObjectPools != null) {
                foreach (var pool in proxy.ObjectPools)
                    ObjectPool.FromXmlProxy(pool, manager);
            }

            return manager;
        }
    }

    public class MetaObjectPoolManager : MetaPoolManager<ObjectPool, Guid, ObjectPoolManager>, IObjectPoolManager
    {
        public TexturePool TexturePool
        {
            get { return GetManager(Default).TexturePool; }
        }
    }
}
