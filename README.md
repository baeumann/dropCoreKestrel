When you wanna do a very simple bare bones C# web application that does support SSL, this project is for you.
It basically is just using the kestrel web server and some minor additions, to create something like a blog engine or however you could call that.

This is nothing for large scale, as this is supposed to be of size that can be stored in memory, for quick access.
So just for your hobby projects. 

You can duplicate it though onto different machines and have a load balancer randomly targeting each machine I guess, to scale this in a red-neck way.

Just take care to adjust it your needs, for example, change the certificate file names to your own, obvious I guess...


The "paragraphs.file" contains your blog posts, I locally got an editing application that can be used to edit this file and create new blog posts in it.
But I am not currently releasing that. You can still manually do that right now. When adhering to the parsing scheme of that file.

Maybe I will do another editor in the future. Or do actual account management so that you could do this online on your web service, but right now I chose not to keep the editing of the content on your hosting machine.
As that is as secure as can get. Because when they are on your machine... I guess you know what I am on about...
