# PaperTrail Feature Showcase Presentation Script

**Speaker Note:** Use this script as a guide or talking points while performing a live demo of the PaperTrail application.

---

## 1. Account Management
"To start things off, let's look at how users interact with PaperTrail on a personal level. We built a comprehensive Account Management system. When a customer or admin visits the site, they can securely register and log in to their own account. 

Once logged in, the user has access to their personal dashboard. Here, they can manage their private data, update their contact information, and navigate their profile settings safely. By handling authentication and authorization properly, we ensure that everyone’s data remains secure and that users only see what they have permission to access."

---

## 2. Ability to Upload Profile Picture
"To make the experience more personalized and engaging, we've implemented a Profile Picture Upload feature within the Account Management section. 

Users can simply choose an image from their device and upload it to their profile. On the backend, our Razor Pages seamlessly intercept this file, validate it to ensure it is a safe image format, and store it cleanly within the server. Immediately after uploading, the UI updates to feature their new custom avatar across their dashboard, making the platform feel unique to every user."

---

## 3. Search Bar
"Next, let's move to the core storefront. With our growing inventory of books and the complexities of many-to-many genres, we needed a way for customers to find exactly what they are looking for instantly. That brings us to our Search Bar. 

As a user types in a book title or keyword and hits search, the application cleanly queries our SQL Server database. It instantly filters out unrelated items and displays only the matching books directly in the storefront grid. This frictionless product discovery is crucial for keeping users engaged and preventing them from getting overwhelmed by a massive catalog."

---

## 4. Transaction System
"Finally, we arrive at the most critical feature of our platform: the Transaction System. Once a user finds the book they want using our built-in search, they can seamlessly proceed to purchase.

During the checkout phase, our system safely processes the transaction. It validates the cart, links the order specifically to the logged-in user's account history, and records the final sale into the database. Immediately after the transaction processes, the user receives a clean confirmation of their order. This feature represents the complete end-to-end journey in PaperTrail—from logging in, to searching for a book, to securely completing a purchase."