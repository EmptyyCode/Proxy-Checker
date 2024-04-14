<P align="center">
<h1>Proxy Checker</h1>
</P>
<br>
<img src="https://github.com/braxton123111/Proxy-Checker/assets/157948097/7784af88-7d68-471b-ac15-23203a5084fd">
<p align="left">
<h1>How To Use</h1>
1 - paste your proxies in the proxies.txt file <br>
2 - run the app <br>
3 - choose the proxy type ( http , https , socks4 , socks5)
4 - choose the amount of the threads ( i recommend not going over 100) <br>
5 - choose the timeout <br>
6 - choose the target ( its google by default , you can choose your own in this format: https://www.google.com/) <br>
</p>
 <br>
<h1>Tips</h1>
1 - if you have a huge proxylist , i recommend using a small timeout and targeting a website which is not sensitive , and then checking the "good" proxies again , but this time on your main target and with a bigger timeout ( for example you can choose a 2 second timeout and target https://webcode.me/ , then save the good proxies and then target your main website with 10 second timeout ( this really saves time )
<br>
<br>
Example : 
you can choose a 2 second timeout and target https://webcode.me/ , then save the good proxies and then target your main website with 10 second timeout 
<br>
<br>
2 - if you are going to write your own checker , please use task factory instead of manual threading , and then use semaphore to limit the amount of running tasks , this makes your code way more readable and you dont even need to write an algorithm
 

