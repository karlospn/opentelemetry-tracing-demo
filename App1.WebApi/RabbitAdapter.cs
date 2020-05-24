using System;
using System.Diagnostics;
using OpenTelemetry.Adapter;
using OpenTelemetry.Trace;

namespace App1.WebApi
{
    public class RabbitAdapter : IDisposable
    {

        private readonly Tracer tracer;
        private readonly DiagnosticSourceSubscriber subscriber;


        private static bool DefaultFilter(string activityName, object arg1, object unused)
        {
            return true;
        }

        public void Dispose()
        {
            this.subscriber?.Dispose();
        }

        public RabbitAdapter(Tracer tracer)
        {
            this.tracer = tracer;
            this.subscriber = new DiagnosticSourceSubscriber(new RabbitMQListener("RabbitMq.Publish", tracer), DefaultFilter);
            this.subscriber.Subscribe();
        }
    }

    public class RabbitMQListener : ListenerHandler
    {
        public RabbitMQListener(string sourceName, Tracer tracer) : 
            base(sourceName, tracer) { }

        public override void OnStartActivity(Activity activity, object payload) => 
            Tracer.StartSpanFromActivity(activity.OperationName, activity);

        public override void OnStopActivity(Activity activity, object payload) => 
            Tracer.CurrentSpan.End();
    }
}

