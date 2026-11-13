# How the code works

**Menu & Actions → TodoManager → TodoStore → sharp-storage → JSON**

- Menu and Actions handle input and display
- Manager handles task and project operations
- Store reads and saves the data

<!--
~55 seconds. 

I organized the code so that:
The menu receives a choice, 
Actions collects input, 
and TodoManager performs the operation. 
TodoStore delegates reading and writing to sharp-storage, 
which persists JSON. 

Keeping these responsibilities separate made it easier to change 
a feature without putting everything in the menu. 
Loaded data is validated before the menu starts.

This also creates the benefit that if we want to change the storage medium,
lets say use a database instead of JSON, we mostly only need to update
sharp-stargage to support whatever storage solution we choose.
-->


