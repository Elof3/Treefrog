﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Treefrog.Presentation.Layers;
using Treefrog.Framework.Model;
using Treefrog.Presentation;
using System.ComponentModel.Composition.Hosting;
using Treefrog.Plugins.Object.Layers;
using Treefrog.Extensibility;
using Treefrog.Render.Layers;

namespace Treefrog.Core
{
    internal class Loader
    {
        [Import]
        LayerPresenterFactoryLoader _layerPresenterFactoryLoader = null;

        [Import]
        CanvasLayerFactoryLoader _canvasLayerFacotryLoader = null;

        public void Compose ()
        {
            AssemblyCatalog catalog = new AssemblyCatalog(System.Reflection.Assembly.GetExecutingAssembly());
            CompositionContainer container = new CompositionContainer(catalog);
            container.SatisfyImportsOnce(this);

            _layerPresenterFactoryLoader.CompleteLoading();
            _canvasLayerFacotryLoader.CompleteLoading();
        }
    }

    /*public interface ILayerPresenterDesc
    {
        Type LayerType { get; }
        Type PresenterType { get; }
        Func<Layer, ILayerContext, LevelLayerPresenter> Create { get; }
    }*/

    /*[Export(typeof(ILayerPresenterDesc))]
    internal class ObjectLayerPresenterDesc : ILayerPresenterDesc
    {
        public Type LayerType {
            get { return typeof(ObjectLayer); }
        }

        public Type PresenterType {
            get { return typeof(ObjectLayerPresenter); }
        }

        public Func<Layer, ILayerContext, LevelLayerPresenter> Create {
            get
            {
                return (layer, context) => {
                    return new ObjectLayerPresenter(context, layer as ObjectLayer);
                };
            }
        }
    }*/

    public interface ILevelLayerPresenterMetadata
    {
        Type LayerType { get; }
        Type TargetType { get; }
    }

    public interface ICanvasLayerMetadata
    {
        Type LayerType { get; }
        Type TargetType { get; }
    }

    [Export]
    internal class LayerPresenterFactoryLoader
    {
        [ImportMany]
        List<Lazy<Func<Layer, ILayerContext, LevelLayerPresenter>, ILevelLayerPresenterMetadata>> _registrants = null;

        public void CompleteLoading ()
        {
            foreach (var entry in _registrants)
                LayerPresenterFactory.Default.Register(entry.Metadata.LayerType, entry.Metadata.TargetType, entry.Value);
        }
    }

    [Export]
    internal class CanvasLayerFactoryLoader
    {
        [ImportMany]
        List<Lazy<Func<LayerPresenter, CanvasLayer>, ICanvasLayerMetadata>> _registrants = null;

        public void CompleteLoading ()
        {
            foreach (var entry in _registrants)
                LayerFactory.Default.Register(entry.Metadata.LayerType, entry.Metadata.TargetType, entry.Value);
        }
    }
}
