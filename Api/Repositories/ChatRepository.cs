using Api.Models.ParameterModels;
using Api.Models.QueryModels;
using Dapper;
using Npgsql;

namespace Api.Repositories;

public class ChatRepository(NpgsqlDataSource source)
{
    private static readonly Uri Uri = new(Environment.GetEnvironmentVariable("FULLSTACK_PG_CONN")!);

    public static readonly string
        ProperlyFormattedConnectionString = string.Format(
            "Server={0};Database={1};User Id={2};Password={3};Port={4};Pooling=true;MaxPoolSize=1",
            Uri.Host,
            Uri.AbsolutePath.Trim('/'),
            Uri.UserInfo.Split(':')[0],
            Uri.UserInfo.Split(':')[1],
            Uri.Port > 0 ? Uri.Port : 5432);

    // pgSQL: SELECT email, messagecontent, sender, messages.id as id, timestamp, room FROM chat.messages join chat.enduser on chat.messages.sender = chat.enduser.id where chat.messages.id<1 and room=1 ORDER BY timestamp DESC LIMIT 5;
    public IEnumerable<MessageWithSenderEmail> GetPastMessages(GetPastMessagesParams getPastMessagesParams)
    {
        using var conn = source.OpenConnection();
        return conn.Query<MessageWithSenderEmail>(@$"
SELECT 
    email as {nameof(MessageWithSenderEmail.email)}, 
    messagecontent as {nameof(MessageWithSenderEmail.messageContent)}, 
    sender as {nameof(MessageWithSenderEmail.sender)}, 
    messages.id as {nameof(MessageWithSenderEmail.id)}, 
    timestamp as {nameof(MessageWithSenderEmail.timestamp)}, 
    room as {nameof(MessageWithSenderEmail.room)} 
FROM chat.messages
join chat.enduser on chat.messages.sender = chat.enduser.id
where chat.messages.id<@{nameof(GetPastMessagesParams.lastMessageId)} and room=@{nameof(getPastMessagesParams.room)} 
ORDER BY timestamp DESC LIMIT 5;", getPastMessagesParams);
    }

    // pgSQL: INSERT INTO chat.messages (timestamp, sender, room, messageContent) values (now(), 1, 1, 'test') returning *;
    public InsertMessageResult InsertMessage(InsertMessageParams insertMessageParams)
    {
        using var conn = source.OpenConnection();
        return conn.QueryFirst<InsertMessageResult>(@$"
INSERT INTO chat.messages (timestamp, sender, room, messageContent) 
values (@{nameof(InsertMessageResult.timestamp)}, 
        @{nameof(InsertMessageResult.sender)}, 
        @{nameof(InsertMessageResult.room)},
        @{nameof(InsertMessageResult.messageContent)}) 
returning 
    timestamp as {nameof(InsertMessageResult.timestamp)}, 
    sender as {nameof(InsertMessageResult.sender)}, 
    room as {nameof(InsertMessageResult.room)}, 
    messageContent as {nameof(InsertMessageResult.messageContent)},
    id as {nameof(InsertMessageResult.id)};", insertMessageParams);
    }

    // pgSQL: insert into chat.enduser (email, hash, salt, isbanned) values ('bla@bla.dk', 'Uhq6WdmkqE+b3R84tTzFAprKxAOto3vhUx0HBG4J524=', 'G/Xx5vBlRMrF+oZcQ1vXiQ==', false) returning *;
    public EndUser InsertUser(InsertUserParams insertUserParams)
    {
        using var conn = source.OpenConnection();
        return conn.QueryFirstOrDefault<EndUser>(@$"
insert into chat.enduser (email, hash, salt, isbanned, isadmin) 
values (
        @{nameof(InsertUserParams.email)}, 
        @{nameof(InsertUserParams.hash)}, 
        @{nameof(InsertUserParams.salt)}, 
        false, false) 
returning 
    email as {nameof(EndUser.email)}, 
    isbanned as {nameof(EndUser.isbanned)}, 
    isadmin as {nameof(EndUser.isadmin)},
    id as {nameof(EndUser.id)};", insertUserParams)
               ?? throw new InvalidOperationException("Insertion and retrieval failed");
    }

    // pgSQL: select count(*) from chat.enduser where email = 'bla@bla.dk';
    public bool DoesUserAlreadyExist(FindByEmailParams findByEmailParams)
    {
        using var conn = source.OpenConnection();
        return conn.ExecuteScalar<int>(@$"
select count(*) from chat.enduser where email = @{nameof(findByEmailParams.email)};", findByEmailParams) == 1;
    }

    // pgSQL: select * from chat.enduser where email = 'bla';
    public EndUser GetUser(FindByEmailParams findByEmailParams)
    {
        using var conn = source.OpenConnection();
        return conn.QueryFirstOrDefault<EndUser>($@"
                        select 
                            email as {nameof(EndUser.email)}, 
                            isbanned as {nameof(EndUser.isbanned)}, 
                            id as {nameof(EndUser.id)},
                            hash as {nameof(EndUser.hash)},
                            salt as {nameof(EndUser.salt)},
                            isadmin as {nameof(EndUser.isadmin)}
                        from chat.enduser where email = @{nameof(FindByEmailParams.email)};", findByEmailParams) ??
               throw new KeyNotFoundException("Could not find user with email " + findByEmailParams.email);
    }

  
    public void ExecuteRebuildFromSqlScript(string? alternativeConnectionString = null)
    {
        using (var conn = source.OpenConnection())
        {
            conn.Execute(@"
/* 
 
 if exists drop schema chat
 */
drop schema if exists chat cascade;
create schema chat;

create table chat.enduser
(
    id       integer generated always as identity
        constraint enduser_pk
            primary key,
    email    text,
    hash     text,
    salt     text,
    isbanned boolean default false,
    isadmin boolean default false
);
create table chat.messages
(
    id             integer generated always as identity
        constraint messages_pk
            primary key,
    messagecontent text,
    sender         integer default '-1':: integer not null
        constraint sender
            references chat.enduser,
    timestamp      timestamp with time zone,
    room           integer
);

INSERT INTO chat.enduser (email, hash, salt, isbanned, isadmin)
values ('bla@bla.dk', 'Uhq6WdmkqE+b3R84tTzFAprKxAOto3vhUx0HBG4J524=', 'G/Xx5vBlRMrF+oZcQ1vXiQ==', false, true);
");
        }
    }
}