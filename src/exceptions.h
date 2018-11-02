#include <stdexcept>
#include <string>

class AddQueryError : public std::runtime_error {
public:
    AddQueryError(const std::string& error);
};

class OpenDatabaseError : public std::runtime_error {
public:
    OpenDatabaseError(const std::string& error);
};

class QueryError : public std::runtime_error {
public:
    QueryError(const std::string& error);
};

class TableNotFoundError : public std::runtime_error {
public:
    TableNotFoundError(const std::string& tableName);
};

class DatabaseCommitError : public std::runtime_error {
public:
    DatabaseCommitError(const std::string& text);
};
