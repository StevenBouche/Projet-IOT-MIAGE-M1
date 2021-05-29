import { TestBed } from '@angular/core/testing';

import { DatastoreDashboardService } from './datastore-dashboard.service';

describe('DatastoreDashboardService', () => {
  let service: DatastoreDashboardService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DatastoreDashboardService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
