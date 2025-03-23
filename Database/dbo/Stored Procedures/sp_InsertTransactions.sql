CREATE   PROCEDURE sp_InsertTransactions
    @TransactionsId INT,
    @SchemeName NVARCHAR(100) NULL,
	@TransactionType NVARCHAR(20) NULL,
	@Units DECIMAL(18,2) NULL,
	@NAV DECIMAL(18,2) NULL,
	@Amount bigint,
	@Date DATE NULL
	AS
	BEGIN

	INSERT INTO Transactions (TransactionsId,SchemeName, TransactionType, Units, NAV, 
	Amount, Date)
    VALUES (@TransactionsId,@SchemeName, @TransactionType, @Units, @NAV, 
	@Amount, @Date);

	END