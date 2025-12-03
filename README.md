When you wanna do a very simple bare bones C# web application that does support SSL, this project is for you.
It basically is just using the kestrel web server and some minor additions, to create something like a blog engine or however you could call that.

This is nothing for large scale, as this is supposed to be of size that can be stored in memory, for quick access.
So just for your hobby projects. 

You can duplicate it though onto different machines and have a load balancer randomly targeting each machine I guess, to scale this in a red-neck way.

Just take care to adjust it to your needs, for example, change the certificate file names to your own, obvious I guess...

When you do not provide your own certificates, it will fall back to developer certificates.

The "paragraphs.file" contains your blog posts, I locally got an editing application that can be used to edit this file and create new blog posts in it.
But I am not currently releasing that. You can still manually do that right now. When adhering to the parsing scheme of that file.

"editor.html" contains a make shift replacement for that. It is an html application to be run locally, it is not provided by the server. 
As I do not wish to make the content editable through the web interface (website) but just keep that to the files of the machine itself.
So only the person having direct access to the machine can alter the content. So it is reduced to the least amount of access vectors possible.

If the RAM is sufficient, everything gets cached. To prevent I/O issues on the machine when requests are made. So use as few pictures and videos as possible.
They get lazy loaded by the client side (but also server side, if no one requests a certain file, it will never get loaded, it doesn't cache everything right away, also just lazy loading). 
The backend resource loading is done pretty rudimentary. No database, direct I/O utilization with caching of once loaded files.

The editor pretty much just handles the base64 encoding and decoding. And allows you for easy insertion of the text marks used for images and video files.
Those files have to be manually transmitted to the target hosting machine. So you are just referencing existing files here. No direct upload to the hosting machine.
