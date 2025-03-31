CREATE TABLE [dbo].[UserRole] (
    [RoleId]   INT           IDENTITY (1, 1) NOT NULL,
    [RoleName] NVARCHAR (20) NULL,
    [IsActive] BIT           NULL,
    PRIMARY KEY CLUSTERED ([RoleId] ASC)
);

