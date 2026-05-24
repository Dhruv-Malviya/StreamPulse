from datetime import datetime
from typing import Optional
from sqlalchemy import CheckConstraint, Index, String, SmallInteger, DateTime, text
from sqlalchemy.orm import Mapped, mapped_column
from sqlalchemy.orm import DeclarativeBase
from .UploadJobStatus import UploadJobStatusEnum

class Base(DeclarativeBase):
    pass

class UploadJob(Base):
    __tablename__ = "upload_jobs"

    __table_args__ = (
        CheckConstraint("upload_job_status >= 0 AND upload_job_status <= 5", name="CK_UploadJob_UploadJobStatus"),
        Index("idx_upload_jobs_modified_at", "modified_at", postgresql_where=text("upload_job_status IN (1, 2, 3)"))
    )
    
    upload_job_id: Mapped[int] = mapped_column(primary_key=True)
    channel_id: Mapped[int] = mapped_column()
    upload_job_status: Mapped[UploadJobStatusEnum] = mapped_column(SmallInteger, server_default=text("0"))
    upload_job_blob_path: Mapped[str] = mapped_column(String(2000))
    upload_job_title: Mapped[str] = mapped_column(String(100))
    upload_job_description: Mapped[Optional[str]] = mapped_column(String(2000))
    upload_job_thumbnail_url: Mapped[Optional[str]] = mapped_column(String(2000))
    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), server_default=text("NOW()"))
    modified_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), server_default=text("NOW()"))

