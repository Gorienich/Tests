using System;
using System.Data;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Library.Enums;

namespace Library.UnitTests
{
    [TestClass]
    public class libraryAccountTests
    {

        private TestContext context;
        public TestContext TestContext
        {
            get
            {
                return context;
            }

            set
            {
                context = value;
            }

        }



        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML",
                    @"|DataDirectory|\TestData\LibraryXML.xml",
                    "Account", 
                    DataAccessMethod.Random)]
        public void LibrarryAccountTestsFromXML()
        {
            // Arrange
            #region Take the account data

            DataRow readerFromXML = TestContext.DataRow.GetChildRows("Account_Reader")[0];
            string readerFirstName = readerFromXML["firstName"].ToString();
            string readerLastName = readerFromXML["lastName"].ToString();
            string readerType = readerFromXML["type"].ToString();
            Enums.ReaderType readerTypeAsEnum = (Enums.ReaderType)Enum.Parse(typeof(Enums.ReaderType), readerType);

            Reader reader = new Reader(readerFirstName, readerLastName, readerTypeAsEnum);

            LibraryAccount libraryAccount = new LibraryAccount(reader); // פותח כרטיס קורא לאדם

            // Read the information about the loan books from the xml, and set it into the library account
            DataRow[] books = TestContext.DataRow.GetChildRows("Account_LoanBook");
            foreach(DataRow row in books)
            {
                // מכאן קוראים את המידע מהקובץ
                string loanBookTypeStr = row["type"].ToString();
                Enums.BookType loanBookTypeEnum = (Enums.BookType)Enum.Parse(typeof(Enums.BookType), loanBookTypeStr);
                string loanBookStatusStr = row["status"].ToString();
                Enums.BookStatus loanBookStatusEnum = (Enums.BookStatus)Enum.Parse(typeof(Enums.BookStatus), loanBookStatusStr);
                               
                Book book = new Book(loanBookTypeEnum, loanBookStatusEnum);
                
                libraryAccount.LoanedBooks.Add(book); // מצרף את הספר שנקרא מהקובץ- לתוך כרטיס הקורא
            }
            // בסוף הלולאה הזו אנו מבינים שקראנו את כל הספרים המושאלים

            // Read the information about the reserved books from the xml, and set it into the library account
            DataRow[] reservedBooks = TestContext.DataRow.GetChildRows("Account_ReserveBook");
            foreach (DataRow row in reservedBooks)
            {
                // מכאן קוראים את המידע מהקובץ
                string reserveBookTypeStr = row["type"].ToString();
                Enums.BookType reserveBookTypeEnum = (Enums.BookType)Enum.Parse(typeof(Enums.BookType), reserveBookTypeStr);
                string reserveBookStatusStr = row["status"].ToString();
                Enums.BookStatus reserveBookStatusEnum = (Enums.BookStatus)Enum.Parse(typeof(Enums.BookStatus), reserveBookStatusStr);

                Book book = new Book(reserveBookTypeEnum, reserveBookStatusEnum);

                libraryAccount.ReservedBooks.Add(book); // מצרף את הספר שנקרא מהקובץ- לתוך כרטיס הקורא
            }

            // When the code came here then we finished reading all the data on the account from the xml,
            // so we have library account with reader information and loan books information and reserved books information
            // ====> we can start the test


            // Read the test data (bookToTest)
            // בעבודה מוגדר בכל פעם לנסות להשאיל ספר בודד, ולכן נניח כי במערך הבא יש רק ספר אחד
            DataRow bookToTestFromXML = TestContext.DataRow.GetChildRows("Account_BookToTest")[0];
            // מכאן קוראים את המידע מהקובץ
            string bookTypeStr = bookToTestFromXML["type"].ToString();
            Enums.BookType bookTypeEnum = (Enums.BookType)Enum.Parse(typeof(Enums.BookType), bookTypeStr);
            string bookStatusStr = bookToTestFromXML["status"].ToString();
            Enums.BookStatus bookStatusEnum = (Enums.BookStatus)Enum.Parse(typeof(Enums.BookStatus), bookStatusStr);

            // נקרא את המידע לגבי חוב הקורא:
            libraryAccount.Dept = Convert.ToDouble(TestContext.DataRow["dept"]);


            Book bookToTest = new Book(bookTypeEnum, bookStatusEnum);

            // Read the expected
            bool expected = Convert.ToBoolean(TestContext.DataRow["expected"]);


            // Act
            bool actual = libraryAccount.LoanBook(bookToTest);
                        
            #endregion


            // Act
            actual = libraryAccount.LoanBook(bookToTest);

            // Assert
            Assert.AreEqual(expected, actual);
                       
        }


        // דוגמאות לבדיקות לתרחישים ספציפיים:

        [TestMethod]
        public void LoanBook_ChildTriesToLoanAvailableChildrenBook_ReturnsTrueAndUpdateList()
        {
            //Arrange
            Reader reader = new Reader("Lior", "dfdsf", ReaderType.Child);
            LibraryAccount libraryAccount = new LibraryAccount(reader);
            libraryAccount.Dept = 0; // אין חוב
            libraryAccount.LoanedBooks = new System.Collections.Generic.List<Book>();
            libraryAccount.ReservedBooks = new System.Collections.Generic.List<Book>();
            Book book = new Book(BookType.ChildrenBook, BookStatus.InTheLibrary);

            bool actual,
                 expected = true;

            // Act
            actual = libraryAccount.LoanBook(book);

            // Assert
            Assert.AreEqual(expected, actual);
            Assert.IsTrue(libraryAccount.LoanedBooks.Contains(book)); // אם פעולת ההשאלה עבדה כנדרש- הספר חייב להיות ברשימת המושאלים
            Assert.IsTrue(book.Status == BookStatus.OutOfTheLibrary); // בדיקה שהסטטוס של הספר השתנה לאחר השאלתו

        }

        [TestMethod]
        public void LoanBook_ChildWithDeptTriesToLoanAvailableChildrenBook_ReturnsFalseAndNotUpdateList()
        {
            //Arrange
            Reader reader = new Reader("Lior", "dfdsf", ReaderType.Child);
            LibraryAccount libraryAccount = new LibraryAccount(reader);
            libraryAccount.Dept = 1; // יש חוב
            libraryAccount.LoanedBooks = new System.Collections.Generic.List<Book>();
            libraryAccount.ReservedBooks = new System.Collections.Generic.List<Book>();
            Book book = new Book(BookType.ChildrenBook, BookStatus.InTheLibrary);

            bool actual,
                 expected = false;

            // Act
            actual = libraryAccount.LoanBook(book);

            // Assert
            Assert.AreEqual(expected, actual);
            Assert.IsFalse(libraryAccount.LoanedBooks.Contains(book)); // אם פעולת ההשאלה נכשלה- הספר לא ברשימת המושאלים
            Assert.IsTrue(book.Status == BookStatus.InTheLibrary); // בדיקה שהסטטוס של הספר לא השתנה 

        }

        [TestMethod]
        public void LoanBook_ChildTriesToGetAvailableAdultBook_ReturnFalseAndNotUpdateList()
        {
            // Arrange
            Reader reader = new Reader("Lior", "df", ReaderType.Child);
            LibraryAccount libAccount = new LibraryAccount(reader);
            libAccount.Dept = 0; // אין חוב
            libAccount.LoanedBooks = new System.Collections.Generic.List<Book>();
            libAccount.ReservedBooks = new System.Collections.Generic.List<Book>();

            Book book = new Book(BookType.AdultBook, BookStatus.InTheLibrary); // זהו הספר שנרצה לקחת

            bool actual;


            // Act
            actual = libAccount.LoanBook(book); // מפעילים את צד הפיתוח

            // Act
            Assert.IsFalse(actual); // שלא התאפשר לילד לקחת ספר מבוגרים
            Assert.IsFalse(libAccount.LoanedBooks.Contains(book)); // שהספר לא הוכנס בטעות לרשימת המושאלים
            Assert.IsFalse(libAccount.ReservedBooks.Contains(book)); // שהספר לא הוכנס בטעות לרשימת המוזמנים
            Assert.AreEqual(book.Status, BookStatus.InTheLibrary); // הספר לא הושאל ולכן הוא נשאר בספרייה
            
        }

        [TestMethod]
        public void LoanBook_ChildTriesToGetReservedBookByOtherChild_ReturnsFalseAndNotUpdateList()
        {
            // Arrange
            Book book = new Book(BookType.ChildrenBook, BookStatus.Reserved); // ספר מוזמן!!

            // Reader1 - the one who reserved that book
            Reader reader1 = new Reader("Yamen", "dfds", ReaderType.Child);
            LibraryAccount libraryAccount1 = new LibraryAccount(reader1);
            libraryAccount1.ReservedBooks.Add(book);

            // Reader2 - anpther child that tries to take the book
            Reader reader2 = new Reader("Safwan", "dsfds", ReaderType.Child);
            LibraryAccount libraryAccount2 = new LibraryAccount(reader2);

            bool actual;


            // Act
            actual = libraryAccount2.LoanBook(book);

            // Assert
            Assert.IsFalse(actual);
            Assert.IsTrue(libraryAccount1.ReservedBooks.Contains(book));
            Assert.IsFalse(libraryAccount2.LoanedBooks.Contains(book));
            Assert.IsFalse(libraryAccount2.ReservedBooks.Contains(book));
            Assert.IsTrue(book.Status == BookStatus.Reserved);  
        }

        [TestMethod]
        [DataRow(ReaderType.Adult, 1, BookType.AdultBook, BookStatus.InTheLibrary, false, DisplayName ="1....")]
        [DataRow(ReaderType.Adult, 0, BookType.AdultBook, BookStatus.InTheLibrary, true, DisplayName = "2....")]
        [DataRow(ReaderType.Child, 0, BookType.AdultBook, BookStatus.InTheLibrary, false, DisplayName = "3....")]
        [DataRow(ReaderType.Child, 0, BookType.ChildrenBook, BookStatus.OutOfTheLibrary, false, DisplayName = "4....")]
        public void LoanBook1(ReaderType readerType, 
                                   double dept, 
                                   BookType bookType, 
                                   BookStatus bookStatus,
                                   bool expected)
        {
            // Arrange
            Reader reader = new Reader(readerType);
            LibraryAccount libraryAccount= new LibraryAccount(reader);
            libraryAccount.Dept= dept;

            Book book = new Book(bookType, bookStatus);

            bool actual; 

            // act
            actual= libraryAccount.LoanBook(book);

            // assert
            Assert.AreEqual(expected, actual);
            

        }


        // רק הדגמה עם חלק מהמשתנים...אפשר להרחיב....
        [TestMethod]
        [DynamicData("ReturnBookTestGenerator", DynamicDataSourceType.Method)]
        public void ReturnBook12(BookStatus bookStatus,
                                bool expected)
        {
            // arrange
            Reader reader = new Reader();
            LibraryAccount account= new LibraryAccount(reader);

            Book book = new Book(bookStatus);
           
            // יוצר מצב שלכרטיס הקורא שלנו יש ספר מושאל
            account.LoanedBooks.Add(book);
            
            bool actual;

            // Act
            actual = account.ReturnBook(book);

            // Assert
            Assert.AreEqual(expected, actual);

            if (expected)
            {
                Assert.IsFalse(account.LoanedBooks.Contains(book));
                Assert.IsFalse(account.ReservedBooks.Contains(book));

                // בודק האם הסטטוס השתנה כהלכה
                if (bookStatus == BookStatus.OutOfTheLibrary)
                    Assert.AreEqual(book.Status, BookStatus.InTheLibrary);
                else if (bookStatus == BookStatus.OutOfTheLibraryAnReserved)
                    book.Status = BookStatus.Reserved;
            }
            else
            {
                // זהו מצב בו אנו מצפים לשקר
                // false

                Assert.AreEqual(book.Status, bookStatus);
            }
        }


        public static object[][] ReturnBookTestGenerator()
        {
            return new[]
            {
                new object[] {BookStatus.OutOfTheLibrary, true}, // Test1
                new object[] {BookStatus.OutOfTheLibraryAnReserved, true}, // Test2
                new object[] {BookStatus.InTheLibrary, false }, // Test3
                new object[] {BookStatus.Reserved, false } // Test4
            };
        }


    }
}
