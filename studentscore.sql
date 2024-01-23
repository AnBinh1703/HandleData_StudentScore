create database Data_Score; 
use Data_Score;
CREATE TABLE SchoolYear (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(100) NOT NULL,
    ExamYear INT NOT NULL,
    Status VARCHAR(50) NOT NULL
);

CREATE TABLE Subject (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Code VARCHAR(10) NOT NULL UNIQUE,
    Name VARCHAR(100) NOT NULL
);

CREATE TABLE Student (
    Id INT PRIMARY KEY IDENTITY(1,1),
    StudentCode VARCHAR(50) NOT NULL UNIQUE,
    SchoolYearId INT NOT NULL,
    Status VARCHAR(50) NOT NULL,
    FOREIGN KEY (SchoolYearId) REFERENCES SchoolYear(Id)
);

CREATE TABLE Score (
    Id INT PRIMARY KEY IDENTITY(1,1),
    StudentId INT NOT NULL,
    SubjectId INT NOT NULL,
    Score DECIMAL(5,2) NOT NULL,
    FOREIGN KEY (StudentId) REFERENCES Student(Id),
    FOREIGN KEY (SubjectId) REFERENCES Subject(Id)
);