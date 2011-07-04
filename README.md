# confgen - a tool for streamlining your web.configs

ASP.net web sites usually have a web.config, if you stage deployments for the various environments, for development, system testing, user acceptance testing and then finally a live release, you'll probably have lots of web.configs, one for each staging environment. This tool ensures that you'll only ever keep one web.config, a master version, and generate the other web.configs from it.

The master config will be named web.master.config, and will (for the most part) be identical to any other web.config file, all the regular configuration elements, all familiar stuff. In fact, it just needs to have an extension of .master.config, so app.master.config will do too. The master file will contain some additional XML attributes to tell confgen which elements to include for which environments. The first of these is the "environments" attribute, containing a comma delimited list of environments to generate for, something like "dev,systest,uat,live". The "for" attribute is placed on all the elements that are different for each environment, and contains another comma delimited list of environments for which that element is to be included. If there is no "for" attribute, it's included for all environments.

## An example would be helpful...

web.master.config:

	<configuration
		xmlns:conf="http://schemas.bbc.com/development/confgen"
		conf:environments="dev,systest,uat,live">
		...
		<appSettings>
			<add key="PendingLoanLifeTimeInDays" value="5"
				 conf:for="live,uat"/>
			<add key="PendingLoanLifeTimeInDays" value="1"
				 conf:for="systest,dev"/>
			<add key="SmtpHost" value="smtp.worldwide.bbc.co.uk"/>
			<add key="LibraryEmailAddress" value="danijel.rahija@bbc.com"
				 conf:for="systest"/>
			<add key="LibraryEmailAddress" value="kash.farooq@bbc.com"
				 conf:for="dev"/>
			<add key="LibraryEmailAddress" value="wwmediaoperations@bbc.com"
				 conf:for="uat,live"/>
			<add key="LibraryEmailSubject" value="Pending Loan Report"/>
		</appSettings>
		...
	</configuration>

Will produce...

web.config (this is dev, a special case):

	<configuration>
		...
		<appSettings>
			<add key="PendingLoanLifeTimeInDays" value="1"/>
			<add key="SmtpHost" value="smtp.worldwide.bbc.co.uk"/>
			<add key="LibraryEmailAddress" value="kash.farooq@bbc.com"/>
			<add key="LibraryEmailSubject" value="Pending Loan Report"/>
		</appSettings>
		...
	</configuration>

web.config.systest:

	<configuration>
		...
		<appSettings>
			<add key="PendingLoanLifeTimeInDays" value="1"/>
			<add key="SmtpHost" value="smtp.worldwide.bbc.co.uk"/>
			<add key="LibraryEmailAddress" value="danijel.rahija@bbc.com"/>
			<add key="LibraryEmailSubject" value="Pending Loan Report"/>
		</appSettings>
		...
	</configuration>

web.config.uat:

	<configuration>
		...
		<appSettings>
			<add key="PendingLoanLifeTimeInDays" value="5"/>
			<add key="SmtpHost" value="smtp.worldwide.bbc.co.uk"/>
			<add key="LibraryEmailAddress" value="wwmediaoperations@bbc.com"/>
			<add key="LibraryEmailSubject" value="Pending Loan Report"/>
		</appSettings>
		...
	</configuration>

web.config.live:

	<configuration>
		...
		<appSettings>
			<add key="PendingLoanLifeTimeInDays" value="5"/>
			<add key="SmtpHost" value="smtp.worldwide.bbc.co.uk"/>
			<add key="LibraryEmailAddress" value="wwmediaoperations@bbc.com"/>
			<add key="LibraryEmailSubject" value="Pending Loan Report"/>
		</appSettings>
		...
	</configuration>

You apply the "for" attribute to elements, so you could apply it to whole sections of your web.config, replacing an entire log4net section for example.
To install, just copy the exe (download latest here) into the folder where your web.configs are usually kept. When you change your web.master.config, run confgen (you can just double click it in explorer) and it will generate all your web.configs. Errors are printed to the console so if you're having trouble, launch it from cmd.

## Variables

To set a variable:

	<conf:var name="name">value</conf:var>

To set a variable for a specific environment:

	<conf:var name="name" conf:for="dev">value</conf:var>

To use a variable, enclose the variable name in braces:

	<endpoint address="net.msmq://{host}/digitalhub.sps.mediaordering.digitalfilequeue" conf:with-vars="true"/>

Also works for text inside elements:

	<address conf:with-vars="true">net.msmq://{host}/digitalhub.sps.mediaordering.digitalfilequeue</address>

The conf:with-vars attribute controls whether to do variable substitution at all. The default is NOT to do substitution so existing braces won't break, making congen 2 fully backwards compatible with congen 1.

So by default:

	<conversionPattern value="%date [%thread] %-5level %logger [%property{NDC}] - %message%newline" />

Still generates:

	<conversionPattern value="%date [%thread] %-5level %logger [%property{NDC}] - %message%newline" />

The conf:with-vars attribute is inherited by sub-elements. Variables are inherited by sub-elements too.
So in the end, configurations like the one following should be much easier to read and write:

	<?xml version="1.0" encoding="utf-8" ?>
	<configuration xmlns:conf="http://schemas.bbc.com/development/confgen" conf:environments="dev,systest1,systest2,uat,live1,live2">
	       <system.serviceModel>
	              <conf:var name="host" conf:for="dev">localhost</conf:var>
	              <conf:var name="host" conf:for="systest1">v-w-dcs-iis01</conf:var>
	              <conf:var name="host" conf:for="systest2">v-w-dcs-test5</conf:var>
	              <conf:var name="host" conf:for="uat">wwlis1029</conf:var>
	              <conf:var name="host" conf:for="live1">wwlapfs1061</conf:var>
	              <conf:var name="host" conf:for="live2">wwlapfs1062</conf:var>

	              <client>
	                     <endpoint
	                           address="net.msmq://{host}/digitalhub.sps.mediaordering.digitalfilequeue"
	                           binding="netMsmqBinding"
	                           bindingConfiguration="msmqBinding"
	                           contract="Bbc.WW.Dcs.Library.Sps.Core.Services.IDigitalFileService"
	                           name="digitalFile" conf:with-vars="true"/>
	              </client>
	       </system.serviceModel>
	</configuration>
