﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IntelligentPlant.BackgroundTasks {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("IntelligentPlant.BackgroundTasks.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Asynchronous.
        /// </summary>
        internal static string BackgroundWorkItem_ItemType_Async {
            get {
                return ResourceManager.GetString("BackgroundWorkItem_ItemType_Async", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Synchronous.
        /// </summary>
        internal static string BackgroundWorkItem_ItemType_Sync {
            get {
                return ResourceManager.GetString("BackgroundWorkItem_ItemType_Sync", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Undefined.
        /// </summary>
        internal static string BackgroundWorkItem_ItemType_Undefined {
            get {
                return ResourceManager.GetString("BackgroundWorkItem_ItemType_Undefined", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {{ Id = &apos;{0}&apos;, Type = &apos;{1}&apos;, DisplayName = &apos;{2}&apos; }}.
        /// </summary>
        internal static string BackgroundWorkItem_StringFormat {
            get {
                return ResourceManager.GetString("BackgroundWorkItem_StringFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Work items cannot be enqueued while the service is stopped..
        /// </summary>
        internal static string Error_CannotRegisterWorkItemsWhileStopped {
            get {
                return ResourceManager.GetString("Error_CannotRegisterWorkItemsWhileStopped", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The service is already running..
        /// </summary>
        internal static string Error_ServiceIsAlreadyRunning {
            get {
                return ResourceManager.GetString("Error_ServiceIsAlreadyRunning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unspecified error.
        /// </summary>
        internal static string Error_UnspecifiedError {
            get {
                return ResourceManager.GetString("Error_UnspecifiedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An unhandled exception occurred in a &apos;{CallbackType}&apos; callback..
        /// </summary>
        internal static string Log_ErrorInCallback {
            get {
                return ResourceManager.GetString("Log_ErrorInCallback", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Work item completed: {WorkItem}.
        /// </summary>
        internal static string Log_ItemCompleted {
            get {
                return ResourceManager.GetString("Log_ItemCompleted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Work item dequeued: {WorkItem}.
        /// </summary>
        internal static string Log_ItemDequeued {
            get {
                return ResourceManager.GetString("Log_ItemDequeued", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Work item enqueued: {WorkItem}.
        /// </summary>
        internal static string Log_ItemEnqueued {
            get {
                return ResourceManager.GetString("Log_ItemEnqueued", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Work item enqueued while background task service is stopped: {WorkItem}. This work item will not be processed until the service is started..
        /// </summary>
        internal static string Log_ItemEnqueuedWhileStopped {
            get {
                return ResourceManager.GetString("Log_ItemEnqueuedWhileStopped", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Work item faulted: {WorkItem}.
        /// </summary>
        internal static string Log_ItemFaulted {
            get {
                return ResourceManager.GetString("Log_ItemFaulted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Work item running: {WorkItem}.
        /// </summary>
        internal static string Log_ItemRunning {
            get {
                return ResourceManager.GetString("Log_ItemRunning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Background task service is running..
        /// </summary>
        internal static string Log_ServiceRunning {
            get {
                return ResourceManager.GetString("Log_ServiceRunning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Background task service has stopped..
        /// </summary>
        internal static string Log_ServiceStopped {
            get {
                return ResourceManager.GetString("Log_ServiceStopped", resourceCulture);
            }
        }
    }
}
