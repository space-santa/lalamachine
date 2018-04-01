#include "exceptions.h"

NoMetaDataException::NoMetaDataException(const std::string& fileName)
    : std::runtime_error(fileName + std::string(" has no metadata.")) {
}

AddQueryError::AddQueryError(const std::string& error) : std::runtime_error(error) {
}

OpenDatabaseError::OpenDatabaseError(const std::string& error) : std::runtime_error(error) {
}

DirectoryNotFoundError::DirectoryNotFoundError(const std::string& rootPath)
    : std::runtime_error(std::string("Directory '") + rootPath + std::string("' not found.")) {
}

QueryError::QueryError(const std::string& error) : std::runtime_error(error) {
}