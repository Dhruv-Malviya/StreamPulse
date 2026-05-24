from enum import Enum

class UploadJobStatusEnum(Enum):
    PENDING = 0
    UPLOADING = 1
    UPLOADED = 2
    PROCESSING = 3
    COMPLETED = 4
    FAILED = 5