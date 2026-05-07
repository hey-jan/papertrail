# PaperTrail Project QA & Interview Guide (Presentation Format)

This document captures detailed answers to role-specific questions for the PaperTrail project. It is structured in a comprehensive, bulleted list format to provide an easy-to-read yet highly detailed presentation script. It aligns with our recent transition to .NET 10, the shift to a Many-to-Many category architecture, and the newly implemented Bestseller features.

---

## 🔹 1. Project Manager

* **What is the main goal of the application?** The main goal of "PaperTrail" is to build a robust, full-stack .NET 10 application designed to manage complex book inventories easily, while providing an intuitive, seamless storefront discovery experience.
* **Who will use the system?** There are two distinct users: Administrators (who use the backend to manage inventory, categories, and bestseller promotions) and Customers (who browse the storefront to filter, search, and purchase books).
* **What are the core features of the app?** The core features for the finished product include a fully searchable storefront with an active search bar, dynamic filtering for multiple book categories, complete account management supporting user profiles and profile picture uploads, and a secure transaction system to handle customer purchases natively.
* **How do you break the project into smaller tasks?** We break the project down by domain layers. For instance, the Database Designer tackles the SQL Server schema, the Lead Developer maps the Entity Framework Core models, the UI/UX team aligns the frontend forms, and QA validates the entire workflow.
* **How will you communicate and track progress?** We communicate through targeted, role-specific documentation and track progress in weekly Agile syncs, ensuring that everyone knows exactly how deep data changes impact their immediate frontend or testing tasks.

---

## 🔹 2. UI/UX Designer

* **How did you design the user interface of the app?** We modernized the book creation forms utilizing OS modifier keys for multiple-select category dropdowns, and we styled administrative data tables with custom UI badges to map out multiple genres cleanly.
* **What tools did you use?** We drove the visual design utilizing **Bootstrap** as our core frontend framework to ensure a highly responsive, mobile-first grid system. We paired this heavily with robust custom HTML/CSS patterns to style our specific components without relying heavily on explicit visual mock-up tools during this sprint.
* **How did you ensure the app is user-friendly?** To optimize pathing, the storefront’s breadcrumb navigation extracts only the *first* assigned category (e.g., `Home > Books > Sci-Fi > Book Title`). This prevents horizontal screen overflow and maintains a clean user flow across devices.
* **What design principles did you follow?** Our core ethos is Simplicity and Bloat Prevention wrapped in a neutral and brown color palette. The warm brown tones tie into the "PaperTrail" branding, while the neutral backgrounds construct a high-contrast, low-fatigue environment for admins and a distraction-free storefront for customers.
* **How did you decide the layout of each screen?** Layout decisions prioritize data summaries. Connected entities are intentionally summarized as minimal badges on the "Manage Books" datatable rather than overflowing into large paragraph formats.

---

## 3. Lead Programmer

* **How did you implement the MVC/Razor Pages architecture?** We enforced a strict separation of concerns into distinct layers separating Database Entities, ViewModels, and UI.
* **How did you divide the system into Model, View, and Controller?** 
  * **Model:** Handled by EF Core Domain Layer POCOs (`Book` and `Category`).
  * **View:** Managed by Razor Pages serving as the Customer Storefront and Admin Panel.
  * **Controller/Handler:** Page Handlers intercept HTTP POSTs, orchestrate database updates, and manipulate LINQ queries prior to rendering the View.
* **What technologies or frameworks did you use?** We operate strictly on .NET 10, deeply integrated with Entity Framework (EF) Core, LINQ, and backed by SQL Server.
* **How did you manage code structure and organization?** **AND** **How did you handle integration between frontend and backend?** We bridged the gap using strictly scaled ViewModels. `BookViewModel` utilizes properties like `List<int> CategoryIds` to safely map frontend HTML array posts to the backend while avoiding dangerous overposting vectors.
* **What was the most difficult feature to implement?** Handling EF Core’s Change Tracking over HTTP POSTs for our Many-to-Many pivot table. We had to explicitly query existing books alongside their categories, cross-reference newly selected IDs, and allow EF Core to safely execute `INSERT` and `DELETE` commands without producing duplicate records.

---

## 4. QA Tester

* **How did you test the system?** We strictly executed Core CRUD Workflows explicitly designed to catch edge cases in both the Admin Panel and the Customer Storefront.
* **What types of testing did you perform?** We executed deep Functional Testing (Create/Read/Update/Delete operations against heavy data loads) and State Validation (applying `Ctrl+F5` forced cache refreshes to verify variable persistence).
* **How did you identify and report bugs?** By utilizing structured "break-test" scenarios, such as explicitly saving a book with zero assigned categories to confirm backend validation intercepts the action.
* **What tools or methods did you use for testing?** We integrated manual UI interactions with direct SQL Server inspections. Upon executing a UI-level delete, QA immediately confirms relation destruction structurally through the database engine.
* **How did you ensure all features are working correctly?** By running tight regressions on edge-case logic, ensuring that if a user filters by one specific category, books mapping to *multiple* categories correctly show up if that specific tag is included.
* **Can you give an example of a bug you found and how it was fixed?** During a hard delete operation on a book, there was a critical architectural risk of triggering a chain cascade that would delete the actual parent Category object. We ran regressions to strictly verify that EF Core correctly limited the cascade deletion to the joining pivot table alone, entirely preserving our master category structures.

---

## 5. Database Designer

* **How did you design the database structure?** The fundamental database structure transitioned from a strict One-to-Many setup into a highly flexible Many-to-Many relationship matrix between `Books` and `Categories`.
* **What entities/tables did you create?** We created the `Books` and `Categories` models and allowed EF Core to automatically scaffold a brand-new internal pivot/join table natively titled `BookCategory`.
* **How did you define relationships between tables?** The `BookCategory` pivot table implements a highly secure composite primary key on `BooksId` and `CategoriesId`. Both act as completely indexed foreign keys enforcing strict `ON DELETE CASCADE` rules.
* **What database system did you use?** We rely directly on SQL Server, leveraging natively mapped types like a raw SQL `BIT` for the new `IsBestseller` column.
* **How did you ensure data integrity and avoid redundancy?** Redundancies are prevented via `ON DELETE CASCADE` protections trimming orphan data. Additionally, our `DbInitializer.cs` data-seeding sequence safely interprets legacy string inputs so initialization runs idempotently, preventing duplicate genre seeds.
* **How does the database connect with the Model in MVC?** We firmly embraced EF Core’s "Convention over Configuration." By simply declaring the navigation property `public virtual ICollection<Category>? Categories` inside the Model, EF seamlessly constructs and manages the complex SQL many-to-many relationship automatically.
