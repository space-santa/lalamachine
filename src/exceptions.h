#include <stdexcept>
#include <string>

class AddQueryError : public std::runtime_error {
public:
    AddQueryError(const std::string& error) : std::runtime_error(error) {
    }
};

class OpenDatabaseError : public std::runtime_error {
public:
    OpenDatabaseError(const std::string& error) : std::runtime_error(error) {
    }
};

class QueryError : public std::runtime_error {
public:
    QueryError(const std::string& error) : std::runtime_error(error) {
    }
};

class TableNotFoundError : public std::runtime_error {
public:
    TableNotFoundError(const std::string& tableName)
        : std::runtime_error(std::string("Table '") + tableName + std::string("' not found.")) {
    }
};

class DatabaseCommitError : public std::runtime_error {
public:
    DatabaseCommitError(const std::string& text) : std::runtime_error(text) {
    }
};
