#include <stdexcept>
#include <string>

class NoMetaDataException : public std::runtime_error {
public:
    NoMetaDataException(const std::string& fileName);
};

class AddQueryError : public std::runtime_error {
public:
    AddQueryError(const std::string& error);
};

class OpenDatabaseError : public std::runtime_error {
public:
    OpenDatabaseError(const std::string& error);
};

class DirectoryNotFoundError : public std::runtime_error {
public:
    DirectoryNotFoundError(const std::string& rootPath);
};