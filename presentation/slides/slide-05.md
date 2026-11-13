# A design decision I'd revisit

**Current:** one JSON file; tasks nested inside projects

Project → Tasks → Task

**Alternative to explore:** separate projects and tasks,
linked by `ProjectId`

Project ← `ProjectId` ← Task

<!--
~1 minute. 

I chose one file with tasks nested under projects because it seemed simpler. 
As I added operations such as moving tasks, 
I had to coordinate changes across project task lists. 
If I redesigned it, I would explore keeping projects and tasks separate 
and linking them with a project ID. 

That is a trade-off to explore, maybe in a future project.

-->
