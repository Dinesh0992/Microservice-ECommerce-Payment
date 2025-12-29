--select * from sys.tables;
use OrderDb;
go
select count(*) as cnt,'InboxState' as tablename from InboxState
UNION
select count(*) as cnt,'Orders' as tablename from Orders
UNION
select count(*) as cnt,'OutboxState' as tablename from OutboxState
UNION
select count(*) as cnt,'OutboxMessage' as tablename from OutboxMessage


select * from Orders

select * from InboxState

select * from OutboxMessage

select * from Orders where RazorpayOrderId like '%Rx1Z6uGxF9ofPD%'