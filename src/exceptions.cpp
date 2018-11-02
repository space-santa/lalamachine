#include "exceptions.h"

AddQueryError::AddQueryError(const std::string& error) : std::runtime_error(error) {
}

OpenDatabaseError::OpenDatabaseError(const std::string& error) : std::runtime_error(error) {
}

QueryError::QueryError(const std::string& error) : std::runtime_error(error) {
}

TableNotFoundError::TableNotFoundError(const std::string& tableName)
    : std::runtime_error(std::string("Table '") + tableName + std::string("' not found.")) {
}

DatabaseCommitError::DatabaseCommitError(const std::string& text) : std::runtime_error(text) {
}
