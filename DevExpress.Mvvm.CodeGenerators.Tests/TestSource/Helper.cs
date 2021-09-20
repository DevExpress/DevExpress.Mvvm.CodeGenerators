using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System;
using System.ComponentModel;

namespace DevExpress.Mvvm.CodeGenerators.Tests {
    static class DoWith {
        static void EventCore<TSender, THandler, TArgs>(
            TSender sender,
            Action action,
            Action<TArgs> eventAction,
            Func<TSender, EventHandler<TArgs>, THandler> subscribe,
            Action<TSender, THandler> unsubscribe,
            int expectedFireCount = 1)
            where TArgs : EventArgs {

            var fireCount = 0;
            EventHandler<TArgs> handler = (o, e) => {
                Assert.AreSame(o, sender);
                eventAction(e);
                fireCount++;
            };

            var h = subscribe(sender, handler);
            try {
                action();
            } finally {
                unsubscribe(sender, h);
            }
            Assert.AreEqual(expectedFireCount, fireCount);
        }

        public static void PropertyChangedEvent(INotifyPropertyChanged inpc, Action action, Action<PropertyChangedEventArgs> eventAction, int fireCount = 1) {
            EventCore(
                inpc,
                action,
                eventAction,
                (o, handler) => {
                    PropertyChangedEventHandler x = (sender, args) => handler(sender, args);
                    inpc.PropertyChanged += x;
                    return x;
                },
                (o, handler) => inpc.PropertyChanged -= handler,
                fireCount
            );
        }
        public static void PropertyChangingEvent(INotifyPropertyChanging inpc, Action action, Action<PropertyChangingEventArgs> eventAction, int fireCount = 1) {
            EventCore(
                inpc,
                action,
                eventAction,
                (o, handler) => {
                    PropertyChangingEventHandler x = (sender, args) => handler(sender, args);
                    inpc.PropertyChanging += x;
                    return x;
                },
                (o, handler) => inpc.PropertyChanging -= handler,
                fireCount
            );
        }
    }
}
