
<!DOCTYPE html>
<html>
<head lang="en">
    <meta charset="UTF-8">
    <title>Why the world needed another AngularJS grid</title>
    <meta name="description" content="A discussion on why I built ag-Grid after experience with ui-grid, ng-grid, jqxGrid and Slickgrid">
    <meta name="keywords" content="angularjs ag-grid grid ui-grid ng-grid jqxgrid slickgrid"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <!-- Bootstrap -->
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.1/css/bootstrap.min.css">
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.1/css/bootstrap-theme.min.css">
    <link href="//maxcdn.bootstrapcdn.com/font-awesome/4.2.0/css/font-awesome.min.css" rel="stylesheet">

    <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.1/js/bootstrap.min.js"></script>

    <link rel="stylesheet" type="text/css" href="/style.css">

    <link rel="shortcut icon" href="http://www.ag-grid.com/favicon.ico" />

</head>

<body ng-app="index" class="big-text">

<nav class="navbar navbar-inverse navbar-fixed-top">
    <div class="container">
        <div class="row">
            <div class="col-md-12 top-header big-text">
                        <span class="top-button-wrapper">
                            <a class="top-button" href="/"> <i class="fa fa-home"></i> Home</a>
                        </span>
                        <span class="top-button-wrapper">
                            <a class="top-button" href="/example.php"> <i class="fa fa-bicycle"></i> Test Drive</a>
                        </span>
                        <span class="top-button-wrapper">
                            <a class="top-button" href="/documentation.php">  <i class="fa fa-book"></i> Documentation</a>
                        </span>
                        <span class="top-button-wrapper">
                            <a class="top-button-selected" href="/media.php"> <i class="fa fa-road"></i> Media</a>
                        </span>
                        <span class="top-button-wrapper">
                            <a class="top-button" href="/forum"> <i class="fa fa-users"></i> Forum</a>
                        </span>
            </div>
        </div>
    </div>
</nav>

<div class="container" style="margin-top: 50px;">

    <div class="row">
        <div class="col-md-12">
            <h2>Why The World Needed Another AngularJS Grid</h2>
        </div>
    </div>

    <div class="row">
        <div class="col-md-8">
            <p>
                Breaking for Christmas at the end of 2014, I left work frustrated.
                We released a new system into production with 'usability issues'
                due to our choice of grid component. Every time we started a new project
                the question 'which grid should we use' came up and each time
                there was debate. Each grid had it's merits, no grid fit
                well. We were on our third project in three years to be using
                <a href="https://angularjs.org/">AngularJS</a>, having tested 5 different grids that 'worked' with
                <a href="https://angularjs.org/">AngularJS</a>.
            </p>
            <p>
                In the beginning we were new to <a href="https://angularjs.org/">AngularJS</a>, so we decided to use
                <a href="http://angular-ui.github.io/ng-grid/">ng-grid</a>. We found the basic interface into <a href="http://angular-ui.github.io/ng-grid/">ng-grid</a> nice and simple,
                an <a href="https://angularjs.org/">AngularJS</a> directive that took a list of columns and a list
                of rows. However we also needed to have checkbox selection and grouping,
                and found to get these items from <a href="http://angular-ui.github.io/ng-grid/">ng-grid</a> required using plugins,
                looking at <a href="http://angular-ui.github.io/ng-grid/">ng-grid</a> source code and hacking around. These we could
                live with (ie our users don't care about these things) but found frustrating
                as developers. Once our tables started to grow in size beyond 20 columns and
                once we introduced pinned columns, the grid gave the user experience
                that the application had stalled - in other words it made the application
                unusable.
            </p>
            <p>
                So for our more complex tables we went with <a href="http://www.jqwidgets.com/jquery-widgets-demo/demos/jqxgrid/">jqxGrid</a>. <a href="http://www.jqwidgets.com/jquery-widgets-demo/demos/jqxgrid/">jqxGrid</a> performed
                much better with large data-sets, but we still found it lacking. It behaved
                a bit clunky, was tricky to extend and customise, and wasn't native to <a href="https://angularjs.org/">AngularJS</a>
                (making it not 'natural' for the <a href="https://angularjs.org/">AngularJS</a> developer). We stuck with <a href="http://www.jqwidgets.com/jquery-widgets-demo/demos/jqxgrid/">jqxGrid</a>,
                taking it as the 'best choice for now'.
            </p>
            <p>
                We also considered <a href="https://github.com/mleibman/SlickGrid">SlickGrid</a>. It's widely adopted, free, and performed
                very well. However it didn't support pinned columns (unless you went
                with a branch) and it wasn't written with <a href="https://angularjs.org/">AngularJS</a> in mind.
            </p>
            <p>
                I realised in 2014 that <a href="http://angular-ui.github.io/ng-grid/">ng-grid</a> was been rewritten, that <a href="http://ui-grid.info/">ui-grid</a> was
                going to be the new <a href="http://angular-ui.github.io/ng-grid/">ng-grid</a> and solve the performance problems.
                So I kept a close eye on <a href="http://ui-grid.info/">ui-grid</a> and was excited in late in 2014 when
                I could download a release candidate of <a href="http://ui-grid.info/">ui-grid</a>. Finally I could say
                goodbye to <a href="http://www.jqwidgets.com/jquery-widgets-demo/demos/jqxgrid/">jqxGrid</a> and move back into the pure-<a href="https://angularjs.org/">AngularJS</a> world...
                I really wanted <a href="http://ui-grid.info/">ui-grid</a> to be my answer...
            </p>
            <p>
                Performance in <a href="http://ui-grid.info/">ui-grid</a> was disappointing. And it had bugs. OK so the bugs I could deal with,
                it was a release candidate after all and bugs are expected in initial releases
                of all software (note: time of writing, 4 months later, it's still
                release candidate), but the performance made our application fail system test.
                That means the performance was so bad our test team deemed the application not suitable
                to be released into production. I was ready to be ui-grids number one fan!! But
                we took it out and put <a href="http://www.jqwidgets.com/jquery-widgets-demo/demos/jqxgrid/">jqxGrid</a> back in :(
            </p>
            <p>
                So <a href="http://www.jqwidgets.com/jquery-widgets-demo/demos/jqxgrid/">jqxGrid</a> was again our grid of choice and I went home for Christmas to mull over it all.
                <a href="https://github.com/mleibman/SlickGrid">SlickGrid</a> showed Javascript was capable of a great fast grid. But I liked the interface
                and native <a href="https://angularjs.org/">AngularJS</a> feel of <a href="http://ui-grid.info/">ui-grid</a>. How hard would it be to match
                <a href="https://github.com/mleibman/SlickGrid">SlickGrid</a> for performance, but keep the <a href="https://angularjs.org/">AngularJS</a> interface??? Can it really
                be that difficult to write a grid that gave me this?
            </p>
            <p>
                So I began my Christmas break pet project entitled <i>"How do you build a lightning fast grid for <a href="https://angularjs.org/">AngularJS</a>"</i>.
            </p>
            <p>
                First step was technology choice. I wrote the following prototypes, wanting to consider every possibility:
                <ul>
                    <li><b>SVG:</b></li> I created a prototype using <a href="http://www.w3.org/Graphics/SVG/">SVG</a> DOM elements. I considered
                    using D3 to interface with the DOM, but for the fastest possible grid, I thought D3
                    could add an unnecessary layer. So I wrote raw Javascript creating <a href="http://www.w3.org/Graphics/SVG/">SVG</a>. I virtualised
                    the rows on a scrolling div. This was interesting for me to program,
                    but I knew where this was going, out of reach of most developers not used to <a href="http://www.w3.org/Graphics/SVG/">SVG</a> and
                    hence customisation was going to be a nightmare. Time for the next prototype...
                    <li><b>Canvas:</b></li> <a href="http://www.w3.org/TR/2dcontext/">Canvas</a> was of interest to me because games are
                    programmed using the HTML <a href="http://www.w3.org/TR/2dcontext/">canvas</a> and games render very fast, with complicated
                    scenes drawn at high frame rates. So I created a <a href="http://www.w3.org/TR/2dcontext/">canvas</a> to act as my viewport,
                    and used sliders to act as scrollbars (ie no native browser scrolling).
                    The position of the sliders indicated to the grid engine what part
                    of the grid to render within the viewable area.
                    This is how 2D scrolling platform games work, deciding what gets rendered
                    in the visible viewport depending on the scroll position. I then used <a href="http://www.w3.org/TR/2dcontext/">canvas</a> 2D drawing API to render the grid cells.
                    This prototype worked very fast but had the following drawbacks: a) looked awful as the styling could not use
                    CSS and layout was crude b) you loose all dom interactivity on elements (ie you
                    cannot attach native browser events to your own <a href="http://www.w3.org/TR/2dcontext/">canvas</a> objects) and c) like <a href="http://www.w3.org/Graphics/SVG/">SVG</a> this option
                    pushed the technology out of reach of the ordinary Javascript
                    developer (unless like me you like programming games).
                    <li><b>Pure AngularJS:</b></li> Next up was what <a href="http://ui-grid.info/">ui-grid</a>
                    did, use Angular JS for everything - almost using the
                    project to showcase <a href="https://angularjs.org/">AngularJS</a>. So I created a grid
                    using about eight directives, one for a cell, one for a row, one for a
                    column header e.t.c. and create my own 'ng-repeat' to virtualise
                    the rows (I did not virtualise the columns, I do not believe this should be
                    done as it is not normal to have a table with hundreds of columns).
                    This created a lot of Angular scopes and some extra levels of div's
                    than I wanted (unnecessary baggage). The result was very AngularJS-esque,
                    performed fine
                    (better than <a href="http://ui-grid.info/">ui-grid</a>, but unfair to make a comparison at this point
                    as I didn't have any complex features in) but didn't perform as well
                    as <a href="https://github.com/mleibman/SlickGrid">SlickGrid</a>. You see, Angular JS is great for form based applications,
                    where you don't have hundreds of bound values laid out in a grid
                    and scrolling. I still use Angular JS as my primary building tool
                    for web applications. However the 'free stuff' you get costs CPU
                    processing and redraw lags (waiting for the digest cycle), and grids
                    require fast redraw when been scrolled and virtualised, so the extra
                    logic that <a href="https://angularjs.org/">AngularJS</a> puts in behind the scenes doesn't work in your
                    favour when building something like a complex grid. So my journey
                    continued, how can a grid be an <a href="https://angularjs.org/">AngularJS</a> component
                    but yet not use <a href="https://angularjs.org/">AngularJS</a>???
                    <li><b>Hybrid AngularJS:</b></li> The final prototype offering, use <a href="https://angularjs.org/">AngularJS</a>
                    to present the grid (so has an <a href="https://angularjs.org/">AngularJS</a> interface like <a href="http://ui-grid.info/">ui-grid</a>) but doesn't
                    use <a href="https://angularjs.org/">AngularJS</a> itself to draw the grid. This incidentally became the
                    evolutionary prototype of what is now ag-Grid. When drawing the grid, not
                    using <a href="https://angularjs.org/">AngularJS</a> means you don't get two-way-binding. However in my
                    experience, the vast majority of grids don't require this, so supporting
                    it is slowing down everything to cater for the few. The performance,
                    to my surprise, was up to <a href="https://github.com/mleibman/SlickGrid">SlickGrid</a>. I say 'to my surprise' as I was
                    expecting it to be harder to come up with a fast grid (if I'd had an
                    inkling I wouldn't of bothered with my <a href="http://www.w3.org/Graphics/SVG/">SVG</a> and <a href="http://www.w3.org/TR/2dcontext/">canvas</a> experiments,
                    but they were fun, if you consider alone on your laptop writing code
                    that will never be used 'fun').
                    But what about <a href="https://angularjs.org/">AngularJS</a>, what if someone did want two-way-binding,
                    or use <a href="https://angularjs.org/">AngularJS</a> to customise a cell? Easy - we can add that as an option!
                    If you want to use <a href="https://angularjs.org/">AngularJS</a> you can, just turn it on and have your rows
                    compiled <a href="https://angularjs.org/">AngularJS</a> style!!!
                </ul>
            </p>
            <p>
                The Hybrid <a href="https://angularjs.org/">AngularJS</a> was the clear winner. The SVG and canvas experiments
                were fun but ended up in my 'home office software graveyard'. With the prototype
                under my belt, I felt I had the workings of a grid worthy of investing more time in,
                so I came up with a list of all the things I wanted in a grid. My list was
                as follows (otherwise known as requirements):
                <ul>
                    <li><a href="https://angularjs.org/">AngularJS</a> Interface</li> - Influenced by <a href="http://angular-ui.github.io/ng-grid/">ng-grid</a>. I wanted it to be simple to
                    pop in a list of rows and columns and off you go. The default settings
                    would cater for everything else.
                    <li>Customisable Cell Editors and Renderers</li> - Influenced by Java Swings JTable.
                    I really liked the power of Swing's JTable and have not come across a grid in Javascript / HTML
                    that gave me similar ease of use.
                    <li>Themable via CSS</li> - Everything has a CSS class, everything can be styled.
                    I wanted our CSS guys to 'go to town' making our most important grids very professional for 'board level presentations'.
                    <li>Pinned columns & Checkbox Selection</li> - These must work smoothly and be supported
                    as part of the core grid out of the box.
                    <li>'Excel like' filtering</li> - Our users were used to Microsoft Excel, where
                    the user can select values from a set. This must work very fast, so the user
                    can easily slice and dice the data with zero wait times.
                    <li>Customisable Filters</li> - Should be easy to add in new filters to the grids data-set.
                    Applications I build typically have custom filters at the top of the grid. I wanted
                    a grid that could easily combine these filters into the core grid filtering.
                    <li>Aggregations and Grouping</li> - Having worked in Business Intelligence,
                    I appreciate the need to handle large data sets. Not just display the data,
                    but be able to 'manage' the data. This means group and aggregate on the fly.
                    In the future I expect large data in the browser to become more common.
                    <li>Zero Dependencies</li> - Not even JQuery, as it's becoming common to not use JQuery
                    in modern web app development.
                    <li>Other stuff</li> - And then it had to support resizable columns,
                    sorting, etc etc
                </ul>
                So I coded up the new grid in time for getting back to work in 2015. I introduced ag-Grid
                to the 'troubled' application. What a breath of fresh air! For the first time we had
                a grid that performed as well as <a href="https://github.com/mleibman/SlickGrid">SlickGrid</a> and had a programming interface native to <a href="https://angularjs.org/">AngularJS</a>.
                The styling and customising worked a treat.
                The grid made our application more responsive than any of the grids that we had used.
                For the first time I felt our grid choice was without compromise.
            </p>
            <p>
                ag-Grid has been released as open source for anyone who finds it helpful.
                We are continuing to use it in my work, so you can expect it to be maintained for
                the foreseeable future.
            </p>
            <p>
                And that is the story of ag-Grid. I hope my contribution back to the community
                can help out if you are suffering the same frustration I used to with the grid choices
                for <a href="https://angularjs.org/">AngularJS</a>. I intend continuing with ag-Grid, making it a great choice
                for <a href="https://angularjs.org/">AngularJS</a>, or even non-<a href="https://angularjs.org/">AngularJS</a> web development. So please spread the word,
                the more people who use it, the better it will become.
            </p>
            <p>
                &nbsp;
            </p>
            <p>
                Niall Crosby, March 2015.
            </p>
            <p style="font-size: 80%;">
                Did you find this post interesting or useful? Please leave a comment below.
            </p>

        </div>

        <div class="col-md-4">
            <div style="font-size: 14px; background-color: #dddddd; padding: 15px;">

                <p>
                    <img src="/niall.png"/>
                </p>
                <p>
                    About Me
                </p>
                <p>
                    I have been writing software all my life! Starting with Assembly, C++ and MFC,
                    moving onto full stack Java / JSP / GWT and now focusing on full stack
                    Java / Javascript / AngularJS.
                </p>
                <p>
                    Consultant working in the financial industry, specialising in web based reporting applications.
                </p>

                <div>
                    <br/>
                    <a href="http://uk.linkedin.com/in/niallcrosby"><img src="/images/linked-in.png"/></a>
                    <br/>
                    <br/>
                    <a href="https://twitter.com/ceolter" class="twitter-follow-button" data-show-count="false" data-size="large">@ceolter</a>
                    <script>!function(d,s,id){var js,fjs=d.getElementsByTagName(s)[0],p=/^http:/.test(d.location)?'http':'https';if(!d.getElementById(id)){js=d.createElement(s);js.id=id;js.src=p+'://platform.twitter.com/widgets.js';fjs.parentNode.insertBefore(js,fjs);}}(document, 'script', 'twitter-wjs');</script>
                </div>

            </div>

            <div style="font-size: 14px; border: 1px solid lightgrey; margin-top: 25px; padding: 15px;">
                This article was was published on:
                <br/>
                <a href="http://dailyjs.com/2015/03/31/angular-grid-react-native/?utm_content=buffer98f7e&utm_medium=social&utm_source=twitter.com&utm_campaign=buffer">DailyJS</a>
                <br/>
                <a href="http://www.dzone.com/links/why_the_world_needed_another_angularjs_grid.html">DZone</a>
                <br/>
                <a href="http://www.reddit.com/r/angularjs/comments/30uel2/why_the_world_needed_another_angularjs_grid/">reddit</a>
                <br/>
                <a href="http://t.co/vpH62y3THW">ng-newsletter</a>
            </div>

            <div class="quote-side-panel">

                <div class="quote-block">
                    <div class="quote">
                        "This is a life saver!

                        Thank you."
                    </div>
                    <div class="under-quote">
                        Vivek 16th May 2015 via Disqus
                    </div>
                </div>

                <div class="quote-block">
                    <div class="quote">
                        "Great work! congratulations!"
                    </div>
                    <div class="under-quote">
                        mrbarletta 31st March 2015 via Disqus
                    </div>
                </div>

                <div class="quote-block">
                    <div class="quote">
                        "I have experienced myself the pain with using the out of the box ng-grid, and I have to say, this grid looks much better, can't wait to see the new features that will get added."
                    </div>
                    <div class="under-quote">
                        Alberto 30th March 2015 via Disqus
                    </div>
                </div>

                <div class="quote-block">
                    <div class="quote">
                        "Hey! Great story! Im happy to find othe people with the same problems as i have, and more happy for finding a great solution. Ui-grid maked me unhealthy."
                    </div>
                    <div class="under-quote">
                        (no name) April 13th 2015 via Disqus
                    </div>
                </div>

                <div class="quote-block">
                    <div class="quote">
                        "Just finished looking through angulargrid.com – very cool!  The demos are incredibly performant."
                    </div>
                    <div class="under-quote">
                        Jim April 2nd 2015 via Email
                    </div>
                </div>

                <div class="quote-block">
                    <div class="quote">
                        "Fantastic mate. Absolutely fantastic work."
                    </div>
                    <div class="under-quote">
                        Micky April 1st 2015 via Disqus
                    </div>
                </div>

                <div class="quote-block">
                    <div class="quote">
                        "Sounds very cool."
                    </div>
                    <div class="under-quote">
                        Brad April 1st 2015 via Disqus
                    </div>
                </div>

                <div class="quote-block">
                    <div class="quote">
                        "I found ag-Grid on the AngularJS subreddit and it's working out great!   I love the speed and the built-in features."
                    </div>
                    <div class="under-quote">
                        Charles 14 Apr 2015 via Email
                    </div>
                </div>

                <div class="quote-block">
                    <div class="quote">
                        "Today a discover your project. Like 10 minutes ago.
                        I am playing around with your ag-Grid. And I love it.
                        I was looking around too until I found your story (NG newsletter).
                        I was trying to create my own grid as well because the other ones are too complex or too simple.
                        Yours is the perfect combination. Easy to use / and well documented!
                        Thank you for sharing this with us angularians."
                    </div>
                    <div class="under-quote">
                        Nelson 3rd April 2015 via Email
                    </div>
                </div>

                <div class="quote-block">
                    <div class="quote">
                        "Excellent article and project. Loved the prototyping scenarios and ultimate hybrid solution. Keep up the great work!"
                    </div>
                    <div class="under-quote">
                        Niall 10th April 2015 via Linked In
                    </div>
                </div>

                <div class="quote-block">
                    <div class="quote">
                        "I'm a big fan of the CSS implementation in angular-grid. The classes are clean, well-structured and named consistently."
                    </div>
                    <div class="under-quote">
                        Paul 16h April 2015 via Github
                    </div>
                </div>

                <div class="quote-block">
                    <div class="quote">
                        "Thanks for suck a amazing angular-grid, we loved that and we are using in our projects."
                    </div>
                    <div class="under-quote">
                        Urvish 27th March 2015 via ag-Grid Forum
                    </div>
                </div>

                <div class="quote-block">
                    <div class="quote">
                        "Wow and thank you. I think you just saved us from creating a React grid component (which I guess they now call an 'element'). We viewed your TestDrive on mobile (android phone and android pad) and it is still very good."
                    </div>
                    <div class="under-quote">
                        Sher 28 March 2015 via ag-Grid Forum
                    </div>
                </div>

                <div class="quote-block">
                    <div class="quote">
                        "Hi, just wanted to let you know that by just switching from ui-grid to your angular-grid I have been able to remove 1788 $watches!"
                    </div>
                    <div class="under-quote">
                        Luis 13th April 2015 via ag-Grid Forum
                    </div>
                </div>

                <div class="quote-block">
                    <div class="quote">
                        "I've showed the demo of angular-grid to about 420 developers last weekend at our Spring into Code workshop and all agreed that this WILL become the de facto grid for AngularJS."
                    </div>
                    <div class="under-quote">
                        Jqravois 13th April 2015 via ag-Grid Froum
                    </div>
                </div>

                <div class="quote-block">
                    <div class="quote">
                        "Amazing work on this! It's easy to use & the code is written in a way that's easy to follow."
                    </div>
                    <div class="under-quote">
                        Rich 15th April 2015 via ag-Grid Forum
                    </div>
                </div>

            </div>

        </div>

    </div>

    <hr/>

    <div id="disqus_thread"></div>
    <script type="text/javascript">
        /* * * CONFIGURATION VARIABLES * * */
        var disqus_shortname = 'angulargrid';

        /* * * DON'T EDIT BELOW THIS LINE * * */
        (function() {
            var dsq = document.createElement('script'); dsq.type = 'text/javascript'; dsq.async = true;
            dsq.src = '//' + disqus_shortname + '.disqus.com/embed.js';
            (document.getElementsByTagName('head')[0] || document.getElementsByTagName('body')[0]).appendChild(dsq);
        })();
    </script>
    <noscript>Please enable JavaScript to view the <a href="https://disqus.com/?ref_noscript" rel="nofollow">comments powered by Disqus.</a></noscript>

    <hr/>

    <footer class="license">
        © Niall Crosby 2015
    </footer>

</div>

</body>

<?php include_once("../analytics.php"); ?>

</html>
