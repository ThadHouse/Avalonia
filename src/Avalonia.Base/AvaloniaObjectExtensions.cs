using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Data;
using Avalonia.Reactive;
using ObservableEx = Avalonia.Reactive.ObservableEx;

#nullable enable

namespace Avalonia
{
    /// <summary>
    /// Provides extension methods for <see cref="AvaloniaObject"/> and related classes.
    /// </summary>
    public static class AvaloniaObjectExtensions
    {
        /// <summary>
        /// Converts an <see cref="IObservable{T}"/> to an <see cref="IBinding"/>.
        /// </summary>
        /// <typeparam name="T">The type produced by the observable.</typeparam>
        /// <param name="source">The observable</param>
        /// <returns>An <see cref="IBinding"/>.</returns>
        public static IBinding ToBinding<T>(this IObservable<T> source)
        {
            return new BindingAdaptor(source.Select(x => (object?)x));
        }

        /// <summary>
        /// Subscribes to a property changed notifications for changes that originate from a
        /// <typeparamref name="TTarget"/>.
        /// </summary>
        /// <typeparam name="TTarget">The type of the property change sender.</typeparam>
        /// <param name="observable">The property changed observable.</param>
        /// <param name="action">
        /// The method to call. The parameters are the sender and the event args.
        /// </param>
        /// <returns>A disposable that can be used to terminate the subscription.</returns>
        public static IDisposable AddClassHandler<TTarget>(
            this IObservable<AvaloniaPropertyChangedEventArgs> observable,
            Action<TTarget, AvaloniaPropertyChangedEventArgs> action)
            where TTarget : AvaloniaObject
        {
            return observable.Subscribe(e =>
            {
                if (e.Sender is TTarget target)
                {
                    action(target, e);
                }
            });
        }

        /// <summary>
        /// Subscribes to a property changed notifications for changes that originate from a
        /// <typeparamref name="TTarget"/>.
        /// </summary>
        /// <typeparam name="TTarget">The type of the property change sender.</typeparam>
        /// /// <typeparam name="TValue">The type of the property..</typeparam>
        /// <param name="observable">The property changed observable.</param>
        /// <param name="action">
        /// The method to call. The parameters are the sender and the event args.
        /// </param>
        /// <returns>A disposable that can be used to terminate the subscription.</returns>
        public static IDisposable AddClassHandler<TTarget, TValue>(
            this IObservable<AvaloniaPropertyChangedEventArgs<TValue>> observable,
            Action<TTarget, AvaloniaPropertyChangedEventArgs<TValue>> action) where TTarget : AvaloniaObject
        {
            return observable.Subscribe(e =>
            {
                if (e.Sender is TTarget target)
                {
                    action(target, e);
                }
            });
        }

        /// <summary>
        /// Subscribes to a property changed notifications for changes that originate from a
        /// <typeparamref name="TTarget"/>.
        /// </summary>
        /// <typeparam name="TTarget">The type of the property change sender.</typeparam>
        /// <param name="observable">The property changed observable.</param>
        /// <param name="handler">Given a TTarget, returns the handler.</param>
        /// <returns>A disposable that can be used to terminate the subscription.</returns>
        [Obsolete("Use overload taking Action<TTarget, AvaloniaPropertyChangedEventArgs>.")]
        public static IDisposable AddClassHandler<TTarget>(
            this IObservable<AvaloniaPropertyChangedEventArgs> observable,
            Func<TTarget, Action<AvaloniaPropertyChangedEventArgs>> handler)
            where TTarget : class
        {
            return observable.Subscribe(e => SubscribeAdapter(e, handler));
        }

        /// <summary>
        /// Binds a property on an <see cref="IAvaloniaObject"/> to an <see cref="IBinding"/>.
        /// </summary>
        /// <param name="target">The object.</param>
        /// <param name="property">The property to bind.</param>
        /// <param name="binding">The binding.</param>
        /// <param name="anchor">
        /// An optional anchor from which to locate required context. When binding to objects that
        /// are not in the logical tree, certain types of binding need an anchor into the tree in 
        /// order to locate named controls or resources. The <paramref name="anchor"/> parameter 
        /// can be used to provice this context.
        /// </param>
        /// <returns>An <see cref="IDisposable"/> which can be used to cancel the binding.</returns>
        public static IDisposable Bind(
            this IAvaloniaObject target,
            AvaloniaProperty property,
            IBinding binding,
            object? anchor = null)
        {
            target = target ?? throw new ArgumentNullException(nameof(target));
            property = property ?? throw new ArgumentNullException(nameof(property));
            binding = binding ?? throw new ArgumentNullException(nameof(binding));

            var metadata = property.GetMetadata(target.GetType()) as IDirectPropertyMetadata;

            var result = binding.Initiate(
                target,
                property,
                anchor,
                metadata?.EnableDataValidation ?? false);

            if (result != null)
            {
                return BindingOperations.Apply(target, property, result, anchor);
            }
            else
            {
                return Disposable.Empty;
            }
        }

        public static IDisposable Bind<T>(
            this IAvaloniaObject target,
            AvaloniaProperty<T> property,
            IObservable<T> source,
            BindingPriority priority)
        {
            if (target is AvaloniaObject ao)
            {
                if (property is StyledPropertyBase<T> styled)
                    return ao.Bind(styled, source, priority);
                else if (priority == BindingPriority.LocalValue)
                    return ao.Bind((DirectPropertyBase<T>)property, source);
                else
                    throw new NotSupportedException("Cannot bind a direct property with non-LocalValue priority.");
            }

            throw new NotSupportedException("Target is not an AvaloniaObject.");
        }

        /// <summary>
        /// Gets a description of a property that van be used in observables.
        /// </summary>
        /// <param name="o">The object.</param>
        /// <param name="property">The property</param>
        /// <returns>The description.</returns>
        private static string GetDescription(IAvaloniaObject o, AvaloniaProperty property)
        {
            return $"{o.GetType().Name}.{property.Name}";
        }

        [Obsolete("Use AvaloniaObject.GetValueByPriority")]
        public static object? GetBaseValue(this IAvaloniaObject o, AvaloniaProperty property, BindingPriority maxPriority)
        {
            return o.GetValueByPriority(property, BindingPriority.Style, maxPriority);
        }

        [Obsolete("Use AvaloniaObject.SetValue without a priority")]
        public static IDisposable? SetValue(
            this IAvaloniaObject o,
            AvaloniaProperty property,
            object value,
            BindingPriority priority)
        {
            if (priority == BindingPriority.LocalValue)
            {
                o.SetValue(property, value);
                return null;
            }
            else
            {
                var source = ObservableEx.SingleValue(o);
                return o.Bind(property, source, priority);
            }
        }

        /// <summary>
        /// Observer method for <see cref="AddClassHandler{TTarget}(IObservable{AvaloniaPropertyChangedEventArgs},
        /// Func{TTarget, Action{AvaloniaPropertyChangedEventArgs}})"/>.
        /// </summary>
        /// <typeparam name="TTarget">The sender type to accept.</typeparam>
        /// <param name="e">The event args.</param>
        /// <param name="handler">Given a TTarget, returns the handler.</param>
        private static void SubscribeAdapter<TTarget>(
            AvaloniaPropertyChangedEventArgs e,
            Func<TTarget, Action<AvaloniaPropertyChangedEventArgs>> handler)
            where TTarget : class
        {
            if (e.Sender is TTarget target)
            {
                handler(target)(e);
            }
        }

        private class BindingAdaptor : IBinding
        {
            private IObservable<object?> _source;

            public BindingAdaptor(IObservable<object?> source)
            {
                this._source = source;
            }

            public InstancedBinding Initiate(
                IAvaloniaObject target,
                AvaloniaProperty targetProperty,
                object? anchor = null,
                bool enableDataValidation = false)
            {
                return InstancedBinding.OneWay(_source);
            }
        }
    }
}
