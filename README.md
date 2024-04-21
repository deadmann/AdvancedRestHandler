# AdvancedRestHandler
A utility code that can be used to request Data from services...

This library is based on my experience and need of working in the past company, taking many services in. I could handle any request so far using it, and every time I felt it doesn't support my need, i tried to expand it.

[![NuGet](https://img.shields.io/badge/nuget-v1.5.0-blue)](https://www.nuget.org/packages/AdvancedRestHandler/)

First install the package using following commands, or download it manually from release section...

```powershell
Install-Package AdvancedRestHandler -Version 1.5.0
```
or
```sh
dotnet add package AdvancedRestHandler --version 1.5.0
```

Then you can use the library.
The requests can be done as simply as calling: 

```C#
AdvancedRestHandler arh = new AdvancedRestHandler([baseUrl], [fixEndOfUrl]);
TResponse response = arh.GetData<TResponse>(url, [options]);
```

in the above code, in the `AdvancedRestHandler` constructor:

 -`baseUrl` is a fixed part of url in a service, which can also be null
 -`fixEndOfUrl` is to automatically attach path as resource in a way that base url of "https://test.test" and calling a operation service of "the-service" had a result of "https://test.testthe-service" or in the other hand, base url of "https://test.test/" and a service url of "/the-service" would result in "https://test.test//the-service", but with this option which is true by default, it will result in "https://test.test/the-service". but due to some services, which they think of '/' as part of the path, and it's existance mean different route, I kept the flag, so it can be turned off.
 
in the `GetData`:

 - `TResponse` is the type of data you are receiving and the incoming json or (i'm not sure if i pars XML, but if supported XML) should be deserialized to
 - `url` depending on the existance of baseUrl can be either the full url to a service, or the versitile part of the path in the service url
 - `options` are modifier that can affect the behaviour of the service
 
Also note that request can be:

 - A premitive
 - A Type
 - A Type inherited from ArhResponse
 - An object of type ArhResponse<[Your Type]>
 - An object of type ArhStringResponse
