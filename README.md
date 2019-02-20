# MATQuestions

This project implements a practical method of producing a safe packing order (if one exists) from a randomly generated shopping bag. It was entirely written using TDD(Test Driven Design), with a focus on readable and maintainable code. 

I used the MSTest testing framework for all my Unit Tests and Moq for my mocking library, due to the simple and logical syntax used in the tests. All tests have logical method names and all assertions have a useful and relevent error message to aid in any debugging.

On top of this the project makes extensive use of Dependency Injection in order to decouple the code, such that all methods are in logical and reusable pieces, with clear boundaries in between objects.

I have 100% code coverage over the programs logic, thanks to the test driven design I used to code the application.

The program first asks you for how many items are in our shopping bag. It then tells you how long it took to calculate the safe ordering, and then calculates a few averages for you, as sometimes it is faster to sort the shopping bag by strength first, before making it safe.


Points of note:

* In the first instance, it is far faster to order the array by strength before making it safe, but on average for some reason it takes exactly the same amount of time as ordering by weight (and much longer than just making it safe initially)
* There are a few places where I had to lose readability due to theoretical limitations - for example, as we cannot divide by zero there is some (basic) handling in the average calculations.
