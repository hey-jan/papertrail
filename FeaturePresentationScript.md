# PaperTrail Feature Showcase Presentation Script

**Speaker Note:** Use this script as a guide or talking points while performing a live demo of the PaperTrail application.

---

## 1. Project Manager Overview (Enya)
"Before the feature walkthrough, here is the project-level view. The main goal of PaperTrail is to provide a complete and secure online bookstore experience where users can discover books, manage their accounts, and complete purchases smoothly from start to finish.

The system is designed for two primary user groups: customers who browse and buy books, and administrators who manage inventory, categories, users, and transaction records.

The core features of the app are account management (registration, login, and profile updates), personalized profile picture upload, fast book search and catalog browsing, and an end-to-end transaction system that validates checkout and stores order history."

---

## 2. Account Management (Enya)
"To start things off, let's look at how users interact with PaperTrail on a personal level. We built a comprehensive Account Management system. When a customer or admin visits the site, they can securely register and log in to their own account. 

Once logged in, the user has access to their personal dashboard. Here, they can manage their private data, update their contact information, and navigate their profile settings safely. By handling authentication and authorization properly, we ensure that everyone’s data remains secure and that users only see what they have permission to access."

---

## 3. Ability to Upload Profile Picture (Jirah)
"To make the experience more personalized and engaging, we've implemented a Profile Picture Upload feature within the Account Management section. 

Users can simply choose an image from their device and upload it to their profile. On the backend, our MVC `AccountController` handles the multipart form submission, writes the file to `wwwroot/uploads/profiles`, updates the user's `ProfilePicture` path in the database, and returns the refreshed profile view. Immediately after uploading, the UI updates to feature their new custom avatar across their dashboard, making the platform feel unique to every user."

---

## 4. Search Bar (John Earl)
"Next, let's move to the core storefront. With our growing inventory of books and the complexities of many-to-many genres, we needed a way for customers to find exactly what they are looking for instantly. That brings us to our Search Bar. 

As a user types in a book title or keyword and hits search, the application cleanly queries our SQL Server database. It instantly filters out unrelated items and displays only the matching books directly in the storefront grid. This frictionless product discovery is crucial for keeping users engaged and preventing them from getting overwhelmed by a massive catalog."

---

## 5. Transaction System (Rowela)
"Finally, we arrive at the most critical feature of our platform: the Transaction System. Once a user finds the book they want using our built-in search, they can seamlessly proceed to purchase.

During the checkout phase, our system safely processes the transaction. It validates the cart, links the order specifically to the logged-in user's account history, and records the final sale into the database. Immediately after the transaction processes, the user receives a clean confirmation of their order. This feature represents the complete end-to-end journey in PaperTrail—from logging in, to searching for a book, to securely completing a purchase."

---

## 6. Customer Order History (Gerly)
"After checkout, users can always review their past purchases through the Customer Order History feature. From the account area, they can open the Orders page and view their own transactions in reverse chronological order, including order number, date, items, quantities, and totals.

On the backend, this is handled in `AccountController` by querying only orders linked to the authenticated user, with related order items and book details loaded for complete display. This gives customers transparent tracking of what they bought and helps build trust because every completed transaction is immediately reflected in their account history."

---

## 7. UI/UX Designer Overview (Jirah)
"For the UI design, I focused on consistency and clarity across the pages users interact with most. I worked on `_Layout.cshtml` for the global navigation and structure, `Home/Index.cshtml` for the storefront presentation, `Account/Profile.cshtml` for account experience, and `wwwroot/css/site.css` for shared styling.

The main tools I used were ASP.NET Core MVC with Razor Views, Bootstrap for responsive layout utilities and reusable components, and custom CSS for branding and page-specific polish.

To ensure user-friendliness, I kept navigation predictable, used clear call-to-action buttons, reduced clutter in high-traffic screens, and added immediate visual feedback on key flows like search and checkout confirmation.

For design principles, I followed visual hierarchy, consistency, contrast, spacing, and accessibility-first readability. I chose a clean, modern bookstore theme with warm neutral backgrounds, deep umber primary tones, and a tan accent because that palette feels editorial and book-centric, reduces visual fatigue during long browsing sessions, and keeps titles, pricing, and call-to-action elements readable without overwhelming the content." 

---

## 8. Database Designer Overview (Rowela)
"For the database structure, I used a normalized relational design centered on catalog, account, and order flow. The custom domain tables I created are `Books`, `Categories`, `BookCategory` (junction table), `Orders`, and `OrderItems`, plus ASP.NET Identity tables such as `AspNetUsers`, `AspNetRoles`, and related auth tables.

The relationships are: many-to-many between books and categories through `BookCategory`; one-to-many from user to orders (`AspNetUsers.Id` -> `Orders.UserId`); one-to-many from order to order items (`Orders.Id` -> `OrderItems.OrderId`); and one-to-many from book to order items (`Books.Id` -> `OrderItems.BookId`). This structure supports flexible categorization, accurate order history, and clean transaction reporting.

The database system I used is Microsoft SQL Server, managed through Entity Framework Core in ASP.NET Core."

---

## 9. Lead Programmer Overview (John Earl)
"As lead programmer, I architected PaperTrail as a layered ASP.NET Core MVC system where each concern is isolated but connected through explicit contracts. In `Program.cs`, I configured the dependency injection graph, SQL Server connectivity through `ApplicationDbContext`, Identity authentication/authorization, session middleware for cart persistence, route mapping, and startup database seeding. This means every request follows a predictable pipeline: middleware handling -> controller action execution -> EF Core query/command -> Razor view rendering.

At the core, I used MVC + EF Core + Identity as the technical backbone. The **Model layer** is defined in `Models/BookModels.cs`, `Models/OrderModels.cs`, and `Models/ApplicationUser.cs`, while `Data/ApplicationDbContext.cs` exposes `DbSet<Category>`, `DbSet<Book>`, `DbSet<Order>`, and `DbSet<OrderItem>` and extends `IdentityDbContext<ApplicationUser>`. This keeps domain entities and identity entities in one coherent unit-of-work so transactional writes (orders + items + stock updates) remain consistent.

For controller orchestration, each controller owns a bounded responsibility: `HomeController.cs` handles catalog discovery (featured books, filtering, sorting, and search suggestions) and projects query state to storefront views; `CartController.cs` manages session-backed cart state with `Extensions/SessionExtensions.cs`, validates inventory before checkout, writes `Order`/`OrderItem` aggregates, decrements stock, and clears session state only after successful persistence; `AccountController.cs` handles Identity flows (register/login/logout), profile mutation, password change, profile image upload, and authenticated order history retrieval; and `AdminController.cs` provides role-restricted management for users, books, categories, and transaction status updates.

I also intentionally separated persistence models from UI transport models. `ViewModels/BookViewModel.cs` and `ViewModels/AccountViewModels.cs` prevent over-posting and keep form concerns localized, so entity classes remain domain-focused while views receive only the fields needed by each screen. This separation makes validation safer, forms easier to evolve, and controller actions clearer to maintain.

On the data side, the relationships are implemented for real-world commerce behavior: many-to-many `Book <-> Category` via `BookCategory`, one-to-many `ApplicationUser -> Orders`, one-to-many `Order -> OrderItems`, and one-to-many `Book -> OrderItems`. The order model includes status transitions (`Confirmed`, `Paid`, `Shipped`, `Completed`, `Cancelled`) and computed order numbering logic in `OrderModels.cs`, while migrations under `Migrations/*.cs` capture schema evolution (books/categories, orders, metadata fields, multi-category support, bestseller flag, and user-name split). `DbInitializer.cs` closes the loop by applying migrations, seeding roles, admin identity, and catalog data from JSON.

The frontend is tightly coupled to these backend contracts without leaking business logic into the UI. `Views/Home/*` consumes filtered/searchable catalog data, `Views/Cart/*` reflects checkout state and totals, `Views/Account/*` binds to account/profile view models, and `Views/Admin/*` maps directly to role-protected CRUD operations. Shared concerns like navigation and auth state are centralized in `Views/Shared/_Layout.cshtml` and related partials, while `wwwroot/css/site.css` and Bootstrap utilities maintain visual consistency.

The files I worked on span full-stack architecture, not isolated features: `Program.cs`, `Data/ApplicationDbContext.cs`, `DbInitializer.cs`, `Controllers/HomeController.cs`, `Controllers/CartController.cs`, `Controllers/AccountController.cs`, `Controllers/AdminController.cs`, `Models/BookModels.cs`, `Models/OrderModels.cs`, `Models/ApplicationUser.cs`, `ViewModels/BookViewModel.cs`, `ViewModels/AccountViewModels.cs`, `Extensions/SessionExtensions.cs`, `Extensions/CurrencyExtensions.cs`, migration files under `Migrations/*.cs`, and Razor/CSS files including `Views/Shared/_Layout.cshtml`, `Views/Home/Index.cshtml`, `Views/Home/Books.cshtml`, `Views/Cart/Checkout.cshtml`, `Views/Account/Profile.cshtml`, `Views/Admin/Books.cshtml`, `Views/Admin/Transactions.cshtml`, and `wwwroot/css/site.css`. That full vertical ownership is how I kept architecture, business rules, data integrity, and UI behavior aligned across the application.

---

## 10. Quality Assurance Tester Overview (Gerly)
"The types of testing I performed were manual functional testing, role-based access testing (Customer vs Admin), and regression testing on critical user flows after each fix.

To ensure all features are working correctly, I executed end-to-end scenarios for the full journey—register/login, browse/search, cart and checkout, profile updates with image upload, order history viewing, and admin-side management pages—then repeated those same flows after changes to confirm no existing feature was broken.

During functional testing, I found that search sometimes returned irrelevant books when users entered keywords. We fixed this by correcting the storefront filtering logic in `HomeController` so search consistently matches book titles and authors before rendering results.

I also found a profile picture issue where uploaded images were not being saved correctly to user profiles in some test runs. We fixed this by ensuring the profile form posts as multipart data, then handling file upload in `AccountController` by saving to `wwwroot/uploads/profiles`, updating the user's `ProfilePicture` path, and persisting it to the database so the new avatar appears after refresh."
