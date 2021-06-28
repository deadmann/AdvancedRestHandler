# AdvancedRestHandler
A utility code that can be used to request Data from services... <br/>

This library is based on my exprience and need of working in the past company, taking many services in. I could handle any request so far using it, and every time I felt it doesn't support my need, i tried to expand it.

[![NuGet](https://img.shields.io/badge/nuget-v1.3.0-blue)](https://www.nuget.org/packages/AdvancedRestHandler/)

First install the package using following commands, or download it manually from release section...

```powershell
Install-Package AdvancedRestHandler -Version 1.4.0
```
or
```sh
dotnet add package AdvancedRestHandler --version 1.4.0
```

Then you can use the library.
The requests can be done as simply as calling: 

```C#
AdvancedRestHandler arh = new AdvancedRestHandler([baseUrl], [fixEndOfUrl]);
TResponse response = arh.GetData<TResponse>(url, [options]);
```

 in the above code, in the `AdvancedRestHandler` constructor: <br/>
 -`baseUrl` is a fixed part of url in a service, which can also be null <br/>
 -`fixEndOfUrl` is to automatically attach path as resource in a way that base url of "https://test.test" and calling a operation service of "the-service" had a result of "https://test.testthe-service" or in the other hand, base url of "https://test.test/" and a service url of "/the-service" would result in "https://test.test//the-service", but with this option which is true by default, it will result in "https://test.test/the-service". but due to some services, which they think of '/' as part of the path, and it's existance mean different route, I kept the flag, so it can be turned off. <br/>
 
 in the `GetData`: <br/>
 -`TResponse` is the type of data you are receiving and the incoming json or (i'm not sure if i pars XML, but if supported XML) should be deserialized to <br/>
 -`url` depending on the existance of baseUrl can be either the full url to a service, or the versitile part of the path in the service url <br/>
 -`options` are modifier that can affect the behaviour of the service <br/>
 
 Also note that request can be: <br/>
 -A premitive <br/>
 -A Type <br/>
 -A Type inherited from ArhResponse <br/>
 -An object of type ArhResponse<[Your Type]> <br/>
 -An object of type ArhStringResponse <br/>
