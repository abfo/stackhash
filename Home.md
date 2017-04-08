# StackHash

![StackHash](Home_stackhash-client-300.png)

StackHash is a rich client/server application that synchronizes with WinQual (the Microsoft service that stores crash reports submitted through Windows Error Reporting).

The StackHash service downloads new product mappings, events, hits and cabs. Synchronization occurs on a schedule and can also be run on demand. This frees you from trying to remember to check the WinQual site.

The client allows you to quickly browse, sort and filter your crash reports. You can start debugging a cab just by pressing F5, or use the scripting feature to automatically debug each new crash dump.

You can run the client and service on a single computer, or run a dedicated StackHash server for your team.

StackHash supports a plugin architecture to take action when products, events and cabs are added from WinQual. Plugins can operate for all data added or updated during synchronization, or can be triggered manually to report specific events.

StackHash includes plugins that support FogBugz integration, email, and running command line tasks. There is also an SDK so you can develop your own plugins.

See the [Documentation](Documentation) tab to get started. If you are not familiar with WinQual and Windows Error Reporting read [Getting Started With WinQual](Home_263275) (PDF) first.

_StackHash art and icons included by kind permission from [LuckyIcon](http://www.luckyicon.com/)._



