CDK
---

May not be necessary?
npm install -g aws-cdk

`cdk init app --language csharp`

Generate and display cloudformation template - `cdk synth`

Deploy the stack with `cdk deploy`

Destroy the stack with `cdk destroy`

### Deployment

After adding docker images and attempting to deploy i got the following error "Error: This stack uses assets, so the toolkit stack must be deployed to the environment (Run "cdk bootstrap aws: //unknown-account/unknown-region")"

The resolution is to run cdk bootstrap, e.g. `cdk bootstrap aws://1234/ap-southeast-2`, this creates a cloudformation stack called CDKToolkit with cdk tools in the environment to build assets. I suspect this only needs to be done once per account.

### CDK

To look up existing resources 


    const vpc = Vpc.fromLookup(this, 'MyExistingVPC', { isDefault: true });
    new cdk.CfnOutput(this, "MyVpc", {value: vpc.vpcId });


### References

https://docs.aws.amazon.com/cdk/latest/guide/ecs_example.html
https://github.com/jeffbryner/aws-cdk-example-deployment/
https://blog.jeffbryner.com/2020/07/20/aws-cdk-docker-explorations.html
https://aws.amazon.com/blogs/compute/hosting-asp-net-core-applications-in-amazon-ecs-using-aws-fargate/
https://blog.tonysneed.com/2019/10/13/enable-ssl-with-asp-net-core-using-nginx-and-docker/
https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-5.0