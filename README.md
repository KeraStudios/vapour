# Vapour API

## Table of Contents
- Introduction
- Installation
- Quick Start
	- Creating Initial App
	- Routes
	- URL Template (TODO)
	- Controllers
	- Models
	- Request Variables (WIP)
	- General Functions

- Advanced Usage
	- Error Handling
	- Logging
	- Best Practices

- FAQ
- ToDos
- Contribute
- Support
- License


## Introduction
Vapour is a VB.NET API built for being as simple and modular as possible without compromising on RESTful API standards. Vapour runs within the existing VB.NET framework for the easiest setup possible.

Features:

- Works inside existing .NET framework
- Simple code
- Very modular to help with debugging and expansion
- Lean code to be as efficient as possible while still be extremely flexible
- Works well with other .NET modules/add ins


## Installation (WIP)

First, create the directory ‘app_code/‘ inside the root of your web app if it isn't there to begin with.
This is the folder where most, or all, of our code will reside. To find out why go to the FAQ section below.
Next, create 'modules/' as a subdirectory inside 'app_code/'.

Lastly, install Vapour by (TODO: rewording/clarification needed?)dropping the GitHub folder 'Vapour/'(:TODO) into the 'app_code/modules/' folder in the root of the web app.
The folder structure should look like the following:
```
app_code/
	modules/
		vapour/
```

NOTE: This is a work in progress so the method of installing/setting up the framework is subject to change.

## Quick Start


### Creating Initial App

To create your first vapour app first create the file 'app.vb' inside the 'app_code/' directory with the following code:
```
Imports Vapour_v1

Public Class MyApp
    Inherits vapour
	
	Overrides Public Sub start()
		
		
	End Sub
	
End Class
```

Next we'll need to create the 'web.config' file in the root of your app with the following content:
````
<configuration>
	<system.webServer>
		<handlers>
			<!-- Note: MyApp is a reference to the class 'NyApp' that was created in the 'app.vb' file. -->
			<add verb="*" path="*" name="MyApp" type="MyApp"/>
		</handlers>
	</system.webServer>
</configuration>
````
Note: if you already have a 'web.config' file you can just merge the handler tags into your web.config file.

Next we'll also create two folders in the 'app_code/' directory called 'controllers/' and 'models/'
respectively. Your folder structure should now look like this:
```
app_code/
	app.vb
	controllers/
	models/
	modules/
		vapour/
web.config
```



### Routes

Routing takes a clients request and filters it down to the correct controller. This is done by matching the request's
method and url to the route's method and url template. When the request matches a route it's associated controller
is executed.

Routes are easy to set up. Only three things are needed:
1. Method
2. Url
3. Controller

Example:
If we want to build a route for the following request (GET: /hello-world/) and afterwards have it run 'controller.helloWorld()' we would do the following:

```
app.route("GET", "/hello-world/", AddressOf controller.helloWorld)
```

When a request comes in for a 'GET' method and for the url '/hello-world/' then 'controller.helloWorld()' will be executed.

Advanced Note: in this example we are using a delegate to overcome a VB restraint which makes it difficult to pass subroutines around like objects.

Our main page should now look like this.

```
Imports Vapour_v1

Public Class MyApp
    Inherits vapour
	
	Overrides Public Sub start()
		
		route("GET", "/hello-world/", AddressOf controllers.helloWorld)
		
	End Sub
	
End Class
```


### URL Template (WIP)

(TODO)


### Controllers

When a route match is found the associated controller will be executed. To build controllers we create a new file in
'/app_code/controllers/'. In our example we will make a file called 'helloWorld.vb' with the following:
```
' We are adding to the controllers class built into the framework.
Public Partial Class controllers
	
	' This is the sub that is called by the route we created earlier.
	Public Sub helloWorld()
		
		' Sends "Hello World" as an api response
		api.display("Hello World!")
		
	End Sub
	
End Class
```


### Models

To continue our prior example, we will create the file 'mymodel.vb' and put it under the
'app_code/models/' directory. Now we want to add the following:

```
imports Vapour_v1

Public Class MyHelloWorld
	
	Public Function getSomething()
		
		return "Hello World! You are Awesome!"
		
	End Function
	
End Class
```

Now that we have created our first model we need to access it in our controller 'helloWorld.vb'.
For that we update our controller code as follows:

```
' We are adding to the controllers class built into the framework.
Public Partial Class controllers
	
	' This is the sub that is called by the route we created earlier.
	Public Sub helloWorld()
		
		' First we create our model.
		Dim myModel as new MyHelloWorld()
		
		' Next we return our models 'getSomething()' function.
		api.display(myModel.getSomething())
		
	End Sub
	
End Class
```


### Request Variables
Vapour utilizes .Net's context handler(TODO: insert reference) to retrieve request variables.
They are all accessible in the main app.vb and controller pages through the api object.

The main types of variables are:
 - Params
	Variables returned from the URL template.
	Usage:
	```
	' For the url template: '/orders/:orderId/'
	api.params("orderId")
	```
	
	For more information on URL variables refer to the URL template section.
	(TODO: add reference to URL template section.)
	
 - Query string
	Regular query string variables.
	Usage:
	```
	api.query("accountCode')
	```

 - Body
	Variables sent along as part of the body of the request. Usually used in POST's but can also
	be used in PUT's and DELETE's. Note, these do no apply to GET requests.
	Usage:
	```
	api.body("firstName")
	```



### General Functions

Inside Vapour there are some general functions that make life easier all round. Here is the list of functions and
their purposes.

#### isNothing:
Often we run across variables that have no value or are DB Null which normally requires a larger conditional
statement to account for the differences. So instead we have created isNothing function that simple returns true
is the variable is nothing or is dbnull.

```
isNothing("test")
' Returns false.
```

#### isSomething:
The opposite of isNothing just avoids the awkward "Not isNothing()" in the code. Returns true if variable has an
actual meaningful value.

```
isSomething("test")
' Returns true.
```

#### isGuid:
(TODO)


## Advanced Usage


### Error Handling

Code will break, that's just a fact, users will enter bad data or something happens unexpectedly, and without a
meaningful way to dealing with those errors things will go disastrously wrong. With that in mind Vapour has a build-in
method of handling these various errors in a meaningful, coherent way that lets the client know about those errors.

Newly created errors are added to a list of errors for that request and are displayed when the api.display(...) is
called. This allows for handling of both hard errors, such as faulty code, as well as soft errors, bad user input.
We create errors in one of two ways:
1) Throw it which will stop the code at that point and display the error in the response:
```
Throw new http500("Something went wrong here")
```

2) Soft errors which will continue the code and display the error in the response:
```
Errors.add(new http500("Something went wrong but will continue the code."))
```

Errors come in several different flavours:
	http400 - Client error
	http404 - Something wasn't found
	http500 - Server side error

Note: error's should follow the http error standards for what kind of error is raised.


### Logging
Often, we developers want a way to record errors and requests. Vapour has a overridable logging subroutine that
allows developers to manage logging in their own fashion.

Usage:
```
Overrides Public Sub logging(clientResponse as object)
	
	' Only do something if the error is greater then 400.
	if clientResponse("status") >= 400 then
		
		' Send email or save to log file.
		
	end if
	
End Sub

```

The clientResponse object is the object that will be sent to the client. This allows us to
log requests based on the information going out, or alternatively, the api object can be used.


### Best Practices

(WIP)

- HTTP variables should not be called within a Model. This will create unnecessary coupling between the
model and the request. Instead the controllers should handle transposing the HTTP variables to the model.
This is also why there is no native access of HTTP variables in Vapour models.


### HTTP Handler Cores

Vapour has two modes built in to handle HTTP requests:
1) Synchronous
This is the default way that vapour handles HTTP requests and means that as users of the api hit it it will
handle those requests one at a time.
If your API won't be getting ten's, or even hundred's, of simultaneous requests, then this handler will do fine.

2) Asynchronous
For cases where an API is expected to have a lot of simultaneous request Vapour has an Asynchronous HTTP handler.

To switch between handlers simply change our main apps inherited class in 'app_code/app.vb' from 'Inherits vapour'
of 'Inherits vapourAsync'

```
Imports Vapour_v1

Public Class MyApp
    Inherits vapour
	' Or alternately we can pick the Async HTTP handler.
	'Inherits vapourAsync
	
	Overrides Public Sub start()
		
		
	End Sub
	
End Class
```

## ToDos
We have a various todo/wishful items that we plan on implementing.
Here is the list:
- Make more flexibility for the user to decide base API structure
- Remove need for the 'app_code/' directory.


## FAQ

Q: Why is all, most, of the code contained inside the 'app_code/' directory?

A: Microsoft .NET has been create in such a way that the common uncompiled code pages cannot be
stored anywhere else except under their defined 'app_code/' directory.
(TODO: Add website reference for source.)



## Contribute

- Source Code: github.com/KeraStudios/vapour/
- Issue Tracker: github.com/KeraStudios/vapour/issues


## Support

If you are having issues, please let us know: thomas@kerastudios.com


## License

The project is licensed under the MIT license:


Copyright (c) 2015 Thomas van Oort

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
