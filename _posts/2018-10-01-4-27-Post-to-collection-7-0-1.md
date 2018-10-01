
---
title : "4.27 Post to a collection"
layout: post
category: "4. OData features"
---

# Post to a collection

Starting in WebAPI OData V7.0.2 [[ASP.NET](https://www.nuget.org/packages/Microsoft.AspNet.OData/7.0.2) | [ASP.NET Core](https://www.nuget.org/packages/Microsoft.AspNetCore.OData/)], adding a new member to a collection of complex type, enums or primitives via a POST requst will be supported.

To create a member in a collection, the client may now send a POST request to that collection’s URL with a payload containing a valid member for the referenced collection. 
For this example, consider that the “Enterprise” entity has a property named - “Employees” that is a collection of Complex Types. A POST request to the collection’s URL will now allow adding a new employee to the collection.   


```
POST /service/Enterprise/1/Employees 
{ 
    "firstName": "New", 
    "lastName": "Intern", 
    "title": "Intern" 
}
```
If the built-in conventional routing is used, this POST request will trigger a controller action PostToEmployeesFromEnterprise(int key, [FromBody] newEmployee) or PostToEmployees(int key, [FromBody] newEmployee) on the EnterpiseController. 

In general, posts requests to ~/Entitiyset/key/Property will trigger an action on the corresponding controller with PostToPropertyFromEntityset or PostToProperty with the key as one argument and a deserialized object from the body. Similarly, post requests to ~/Entityset/key/Property/Cast will trigger PostToPropertyOfCastFromEntityset or PostToPropertyOfCast. 
