# Yawhg Clone

An attempt to make a multiplayer visual novel in the style of the [_“The Yawhg”_](https://store.steampowered.com/app/269030/The_Yawhg/) by [Damian Sommer](https://twitter.com/damiansommer) and [Emily Carroll](http://www.emcarroll.com/).

----

> 2022 Commentary
>
> This project ended up turning into an exercize in dynamic content management
> scripting language design.
>
> The greatest shortcoming I saw with _The Yawhg_ is that there will be no
> more content for it, likely ever. So, I looked into how to set up the project
> so that it could potentially support modding, or at least DLC, leading me to
> learn Unity's (at the time) latest tools for asset handling. There was also
> a randomization element to what scenes would be used, so I wrote in support
> have scenes fetched and loaded asynchronously, allowing them to be streamed
> in as the players read.
>
> Pre-generating the final list could have also potentially worked and more
> simply, except that player choice effects what potential scenes can be
> picked, and frankly it was more technically interesting to me at the time.
>
> As for the scripting language, I wanted to be able to have my friends
> contribute scenes to the game without them needing to know how to work with
> Unity, so once the core design was in place, I put together an interpreter
> that could populate a template scene using only text in a markdown-like
> format. Another reason for the dynamic asset fetching as images could be
> referenced in a script.
>
> It's been a long time since I shelved it (life priorities shifted), but
> almost all of the core systems were in place. It just needed content and
> presentation, I believe.
>
> Maybe someday.
