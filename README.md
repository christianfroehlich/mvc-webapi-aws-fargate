Web Api and MVC hosting on AWS Fargate
--------------------------------------

This repository contains examples of asp.net applications containerised and being hosted using AWS Fargate. They include accompanying AWS CDK stacks to deploy them to AWS.

## Containers

The general setup of the containers are largely based off an excelent article by [Tony Sneed](https://blog.tonysneed.com/2019/10/13/enable-ssl-with-asp-net-core-using-nginx-and-docker/). 

The solution features 2 containers, one to host nginx and the other your asp.net application.

Nginx is a common web server with a low footprint that supports a large amount of concurrent connections. It is recommended to use nginx as the .net kestrel web server is not suitable for production.

In the examples i have used nginx primarily as a reverse proxy and ssl terminator, however as you can see i have also used it for static website hosting. 

The containers are networked in a bridged configuration locally, however use the `Awsvpc` networking mode in Fargate and connect over a local loopback. 

### Running locally

You can run the containers locally using docker-compose.

Build with `docker-compose build`

Run together (in a default rbidge network) with `docker-compose up -d`

Bring it down with `docker-compose down`

### Deploying to AWS

You can deploy the infrastructure using the CDK.

Install the CDK `npm install -g aws-cdk`

Bootstap the CDK in your AWS environment `cdk bootstrap aws://1234/ap-southeast-2` where 1234 is your account id. This will deploy resources required by the CDK and only needs to be performed once.

Generate and display cloudformation template `cdk synth`

Deploy the stack with `cdk deploy`

Destroy the stack with `cdk destroy`

## AWS Architecture

The AWS architecture is based off the reference architecture defined [here](https://aws.amazon.com/blogs/compute/hosting-asp-net-core-applications-in-amazon-ecs-using-aws-fargate/).

In the examples provided here the containers are defined in the same fargate task, this is a simple approach that allows the servers to communicate securely over a local loopback network without any Cors issues.
The limitation of this is that the containers cannot be scaled independantly if the load requirements are different for nginx and your api for example. In such a situation you may need to define the containers in separate tasks and use Service Discovery.

## ASP.NET Considerations

You must add the following forwarded header configuration to the top of Startup.cs.

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
    
HTTP traffic will be proxied to your application without encyption over port 5000 however this will be transparent to your application.

If you serve a static website using nginx you will be able to access your asp.net api via 127.0.0.1 (or as otherwise configured).