# Azure Functions
### Designed to extend the functionality of statically hosted sites on Azure Blob Storage

# Static sites
> TODO: The benefit of static sites

# The user story
Easy to explain, since this is my story, or rather the users of my web site's story.

> *As a user of the site at 'akhvacllc.com', I want to be able to provide my contact information and a brief description of a heating or air conditioning service I may be interested in.*

# The developer story
There's always more to the story, and you'll need the developer side of the story to understand how a seemingly simple user story translates into a complex set of requirements.

> *As a web developer I need to create a contact form for a site where the end user should be able to provide their first name, last name, email address, phone number and a brief message, and have that data emailed to the configured recipient quickly and reliably so they can be contacted.*

# The feature requirements

This is where it gets tricky! Because you absolutely CAN create this feature in Azure Functions with a single function and it will work just fine. However, this is not a 'Hello World' intro, and this is intended to be production ready, so I have more requirements.

I'll list the requirements in order from the end user submitting the form, to me receiving the data in my mailbox.

1. Client side validation
    - first name is required and must be 3 or more characters
    - last name is required must be 3 or more characters
    - email address is required and must have an '@' preceded by at least one character.
      - Yes, 'a@' is acceptable, on the client side and I'll tell you why soon.
      - No, no 'confirm email address', argument could go either way.
    - message is required and must be at 10 or more characters.
2. Server side validation
    - After the data is submitted, I'm going to validate it again. There is no guarantee that the data (A) came from my form (B) wasn't modified by malicious code in the browser or while en-route. *puts on tinfoil hat* TRUST NO ONE. 
3. Email confirmation (server request)
    - Remember when I said 'a@' is good enough validation for the client side? That's mostly because validating an email address is hard.  If you search the interwebs for a Regular Expression pattern that validates emails, you're going to have to wade through at least 100 of them and MAYBE one or two will work (unless you know what you're looking for) because the spec (RFC) for an email address is obnoxiously complicated and we can take a much easier and more accurate path.
    - Not only do I have client side and server side validation, I'm also going to CONFIRM without a doubt that the end user provided at least a valid email address by sending them an email with a link they must click on to continue the process.
4. Email confirmation (end user)
    - When the end user receives the email, they'll get a friendly message asking them to confirm their form submission with a confirmation link.
5. Send confirmed form data
    - Once the end user clicks the link, since they must have a valid email address, the server will now forward that data to my mailbox in another friendly email.
6. Cleanup
    - It is inevitable that robots will fill out this form with invalid or irrelevant data. So we need to clean this up.  Every hour, I need a function which will prune any request that has not been confirmed within a 48 hour period.

# Concepts covered
1. Requirements vs. Details

Notice in the user story and requirements, at no time did I mention Azure, or .NET Core or any platform or coding language?  I can't stress this enough those aren't **requirements**. Language, platforms and such are all implementation details.

1. Details

Digressing from the rant above, I will be using C#, Azure Functions, Visual Studio 2019, and Azure DevOps.

2. Server Side Validation

Just because something is 'serverless', doesn't mean you can opt out of server side validation! 

3. Command and Query Responsibility Segregation (CQRS)

Lightly touching on CQRS, not diving into event sourcing or anything like that, however, there is a clear distinction between the 'read model' and the 'write model', since the write model enforces validation using `DataAnnotations` which I'll cover when we get to the code.

4. Messaging

Since Azure Functions are small bits of code living in the wild (think of a single method on a class, that doesn't violate Single Responsibility Principal (SRP), we can the low latency Functions to complete complex tasks through messaging.

5. Production Ready

Feel free to use this in production. No seriously, this isn't a 'Hello World'. I designed this to be production ready.


    
    
    