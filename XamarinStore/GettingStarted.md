## Getting started with Simple.OData.Client

To use Simple.OData.Client import its namespace in the source file where you will write code consuming data from and OData feed:

<pre>using Simple.OData.Client;</pre>

Create an instance of ODataClient by passing an OData service URL:

<pre>var client = new ODataClient("http://packages.nuget.org/v1/FeedService.svc/");
</pre>

Now you can access data from the OData server using either basic or fluent API. Basic and typed fluent APIs are supported on all platforms, and dynamic fluent API is supported on Windows, Windows Phone and Android.

Example of basic API syntax:

<pre>var packages = client.FindEntries("Packages?$filter=Title eq 'Simple.OData.Client'");
foreach (var package in packages)
{
    Console.WriteLine(package["Title"]);
}</pre>

Example of a typed fluent API syntax (supported on all platforms):

<pre>var packages = client
    .For(<Packages>)
    .Filter(x => x.Title == "Simple.OData.Client")
    .FindEntries();
foreach (var package in packages)
{
    Console.WriteLine(package.Title);
}</pre>

Example of a dynamic fluent API syntax (not supported on iOS):

<pre>var x = ODataDynamic.Expression;
var packages = client
    .For(x.Packages)
    .Filter(x.Title == "Simple.OData.Client")
    .FindEntries();
foreach (var package in packages)
{
    Console.WriteLine(package.Title);
}</pre>

## Other Resources

* [Wiki pages](https://github.com/object/Simple.OData.Client/wiki)
* [Source Code Repository](https://github.com/object/Simple.OData.Client)
